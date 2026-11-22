namespace PaymentGateway.Api.Models.Requests;

public class PostPaymentToAcquiringBankRequest
{
    public required string Card_Number { get; set; }
    public required string Expiry_Date { get; set; }
    public required string Currency { get; set; }
    public int Amount { get; set; }
    public required string Cvv { get; set; }
}
