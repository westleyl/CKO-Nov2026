namespace PaymentGateway.Api.Models.Responses;

public class PostPaymentToAcquiringBankResponse
{
    public bool Authorized { get; set; }
    public string Authorization_code { get; set; }   
}
