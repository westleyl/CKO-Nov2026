using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public static class PaymentValidator
{
    private static Dictionary<string, string> _validCurrencies = new Dictionary<string, string>
    {
        { "GBP", "British Pound" },
        { "USD", "US Dollar" },
        { "EUR", "Euro" }
    };

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

        // expiry month and year
        if (paymentRequest.ExpiryMonth < 1 || paymentRequest.ExpiryMonth > 12)
        {
            validationErrors.Add("Expiry month must have a value between 1 and 12");
        }
        else
        {
            var expiryDate = new DateTime(paymentRequest.ExpiryYear, paymentRequest.ExpiryMonth, 1).AddMonths(1);
            if (expiryDate < DateTime.Now)
            {
                validationErrors.Add("Expiry date based on  year and month must be in the future");
            } 
        }
        
        // currency code
        if (string.IsNullOrEmpty(paymentRequest.Currency))
        {
            validationErrors.Add("Currency is required");
        }
        else if (paymentRequest.Currency.Length != 3)
        {
            validationErrors.Add("Currency must be a 3 character ISO 4217 currency code and by a supported currency");
        }
        else if (!_validCurrencies.ContainsKey(paymentRequest.Currency))
        {
            validationErrors.Add("Currency must be a 3 character ISO 4217 currency code and by a supported currency");
        }

        // Amount
        if (paymentRequest.Amount < 0)
        {
            validationErrors.Add("Amount must be greater than zero");
        }

        // Cvv
        if (string.IsNullOrEmpty(paymentRequest.Cvv))
        {
            validationErrors.Add("Cvv is required");
        }
        else if(paymentRequest.Cvv.Length < 3 || paymentRequest.Cvv.Length > 4)
        {
            validationErrors.Add("Cvv must contain between 3 and 4 numeric characters");
        }
        else if(!paymentRequest.Cvv.All(char.IsDigit))
        {
            validationErrors.Add("Cvv must contain numeric characters (0-9) only");
        }


        return validationErrors.Count == 0;
    }
}