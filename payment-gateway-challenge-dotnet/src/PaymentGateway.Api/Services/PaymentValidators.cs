using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public static class PaymentValidators
{
    public static bool isValid(PostPaymentRequest paymentRequest, out List<String> validationErrors)
    {
        validationErrors = [];

        if (string.IsNullOrEmpty(paymentRequest.CardNumber))
        {
            validationErrors.Add("Card number is required");
        }
        else if(paymentRequest.CardNumber.Length < 14 || paymentRequest.CardNumber.Length > 19)
        {
            validationErrors.Add("Card number must contain between 14 and 19 numeric characters");
        }
        else if(!paymentRequest.CardNumber.All(char.IsDigit))
        {
            validationErrors.Add("Card number must contain numeric characters (0-9) only");
        }

        return validationErrors.Count == 0;
    }
}