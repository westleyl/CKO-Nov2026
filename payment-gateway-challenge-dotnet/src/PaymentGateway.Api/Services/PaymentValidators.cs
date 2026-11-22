using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public static class PaymentValidators
{
    public static bool isValid(PostPaymentRequest paymentRequest, out List<String> validationErrors)
    {
        validationErrors = [];

        // cardNumber checks - mutally exclusive checks 
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

        // expiry month
        if (paymentRequest.ExpiryMonth < 1 || paymentRequest.ExpiryMonth > 12)
        {
            validationErrors.Add("Expiry month must have a value between 1 and 12");
        }

        return validationErrors.Count == 0;
    }
}