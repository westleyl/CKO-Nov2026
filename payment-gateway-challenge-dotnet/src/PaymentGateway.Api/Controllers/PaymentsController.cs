using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;

    private static HttpClient _acquiringBankApi;

    public PaymentsController(PaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;

        // would locate this from some service registry / via dependency injection into this controller
        _acquiringBankApi = new HttpClient();
        _acquiringBankApi.BaseAddress = new System.Uri("http://localhost:8080/payments");
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync(PostPaymentRequest request)
    {
        var isValid = PaymentValidator.isValid(request, out List<string> validationErrors);
        if (!isValid)
        {
            return new BadRequestObjectResult(validationErrors);
        }

        var acquiringBankRequest = new PostPaymentToAcquiringBankRequest
        {
            Card_Number = request.CardNumber,
            Expiry_Date = request.ExpiryMonth.ToString("D2") + "/" + request.ExpiryYear.ToString(),
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };

        HttpResponseMessage bankResponse = await _acquiringBankApi.PostAsJsonAsync("", acquiringBankRequest);

        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = request.ExpiryYear,
            ExpiryMonth = request.ExpiryMonth,
            Amount = request.Amount,
            CardNumberLastFour = Convert.ToInt32(request.CardNumber.Substring(request.CardNumber.Length - 4)),
            Currency = request.Currency
        };

        if (bankResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) 
        {
           payment.Status = Models.PaymentStatus.Rejected;  
        }
        else if (bankResponse.StatusCode == System.Net.HttpStatusCode.OK)
        {
           PostPaymentToAcquiringBankResponse acquiringBankResponse = await bankResponse.Content.ReadFromJsonAsync<PostPaymentToAcquiringBankResponse>();
           if (acquiringBankResponse.Authorized)
            {
                payment.Status = Models.PaymentStatus.Authorized;
            }
            else
            {
                payment.Status = Models.PaymentStatus.Declined;   
            }           
        }
        else
        {
            payment.Status = Models.PaymentStatus.Rejected;  
        }

        _paymentsRepository.Add(payment);

        return new CreatedAtActionResult(
            actionName: nameof(GetPaymentAsync),
            controllerName: "Payments",
            routeValues: new { id = payment.Id },
            value: payment);
    }

    // Needed to use ActionName attribute to allow PostPaymentAsync to get correct URI
    // for Location header from code (instead of hard coding URI that may change)
    // see https://www.josephguadagno.net/2020/07/01/no-route-matches-the-supplied-values
    [HttpGet("{id:guid}")]
    [ActionName(nameof(GetPaymentAsync))]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        if (payment == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(payment);
    }
}