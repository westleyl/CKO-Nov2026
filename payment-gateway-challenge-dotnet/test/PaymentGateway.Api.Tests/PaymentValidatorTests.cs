using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsValidatorTests
{
    private PostPaymentRequest CreateValidPaymentRequest()
    {
        // note: minimum card number length of 14 (can be up to 19)
        return new PostPaymentRequest
        {
            CardNumber = "12345678901234",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1,
            Amount = 1000,
            Currency = "GBP",
            Cvv = "1234"
        };
    }
    
    [Fact]
    public async Task CardNumber_IsRequired()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.CardNumber = String.Empty; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Card number is required", validationErrors);
    }

    [Fact]
    public async Task CardNumber_ContainsOnlyNumericCharacters()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.CardNumber = "1234567890ABCD"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Card number must contain numeric characters (0-9) only", validationErrors);
    }
    
    [Fact]
    public async Task CardNumber_IsTooShort()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.CardNumber = "1234567890123"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Card number must contain between 14 and 19 numeric characters", validationErrors);
    }

    [Fact]
    public async Task CardNumber_IsMinimumLength()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.CardNumber = "12345678901234"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task CardNumber_IsMaximumLength()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.CardNumber = "1234567890123456789"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task CardNumber_IsTooLong()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.CardNumber = "12345678901234567890"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Card number must contain between 14 and 19 numeric characters", validationErrors);
    }

    [Fact]
    public async Task ExpiryMonth_NotBetween1And12()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.ExpiryMonth = 0;

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        if (!isValid)
        {
            paymentRequest.ExpiryMonth = 13;
            isValid = PaymentValidators.isValid(paymentRequest, out validationErrors);
        }

        // Assert
        Assert.False(isValid);
        Assert.Contains("Expiry month must have a value between 1 and 12", validationErrors);
    }

    [Fact]
    public async Task ExpiryMonth_Between1And12()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.ExpiryMonth = 1;

        // Act
        var isValid = false;
        for (int expiryMonth = 1; expiryMonth <= 12; expiryMonth++)
        {
            paymentRequest.ExpiryMonth = expiryMonth;
            isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
            if (!isValid)
            {
                break;
            }
        }

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task CombinationOfExpiryYearAndMonth_InPast()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.ExpiryYear = DateTime.Now.AddMonths(-1).Year;
        paymentRequest.ExpiryMonth = DateTime.Now.AddMonths(-1).Month;

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Expiry date based on  year and month must be in the future", validationErrors);
    }

    [Fact]
    public async Task CombinationOfExpiryYearAndMonth_InFutureCurrentMonth()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.ExpiryYear = DateTime.Now.Year;
        paymentRequest.ExpiryMonth = DateTime.Now.Month;

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task CombinationOfExpiryYearAndMonth_InFutureNextYear()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.ExpiryYear = DateTime.Now.AddYears(1).Year;
        paymentRequest.ExpiryMonth = DateTime.Now.Month;

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task CurrencyCode_IsRequired()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.Currency = String.Empty; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Currency is required", validationErrors);
    }

    [Fact]
    public async Task CurrencyCode_IncorrectFormat()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();

        // Act
        paymentRequest.Currency = "USDC";
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Currency must be a 3 character ISO 4217 currency code and by a supported currency", validationErrors);
    }


    [Fact]
    public async Task CurrencyCode_ISOCodeNotSupportedCurrency()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();

        // Act
        paymentRequest.Currency = "CAD";
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Currency must be a 3 character ISO 4217 currency code and by a supported currency", validationErrors);
    }


    [Fact]
    public async Task CurrencyCode_IsValid()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();

        // Act
        paymentRequest.Currency = "USD";
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        if (isValid)
        {
            paymentRequest.Currency = "GBP";
            isValid = PaymentValidators.isValid(paymentRequest, out validationErrors);
        }
        if (isValid)
        {
            paymentRequest.Currency = "EUR";
            isValid = PaymentValidators.isValid(paymentRequest, out validationErrors);
        }

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task Amount_LessThanZero()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.Amount = -1;

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Amount must be greater than zero", validationErrors);
    }

    [Fact]
    public async Task Amount_GreaterOrEqualToZero()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();

        // Act
        paymentRequest.Amount = 0;
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        if (isValid)
        {
            paymentRequest.Amount = 1;
            isValid = PaymentValidators.isValid(paymentRequest, out validationErrors);
        }
        if (isValid)
        {
            paymentRequest.Amount = 1050;
            isValid = PaymentValidators.isValid(paymentRequest, out validationErrors);
        }

        // Assert
        Assert.True(isValid);
    }

   [Fact]
    public async Task Cvv_IsRequired()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.Cvv = String.Empty; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Cvv is required", validationErrors);
    }

    [Fact]
    public async Task Cvv_ContainsOnlyNumericCharacters()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.Cvv = "1AB"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Cvv must contain numeric characters (0-9) only", validationErrors);
    }
    
    [Fact]
    public async Task Cvv_IsTooShort()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.Cvv = "01"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Cvv must contain between 3 and 4 numeric characters", validationErrors);
    }

    [Fact]
    public async Task Cvv_IsMinimumLength()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.Cvv = "012"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task Cvv_IsMaximumLength()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.Cvv = "1234"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task Cvv_IsTooLong()
    {
        // Arrange
        var paymentRequest = CreateValidPaymentRequest();
        paymentRequest.Cvv = "00123"; 

        // Act
        var isValid = PaymentValidators.isValid(paymentRequest, out List<string> validationErrors);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Cvv must contain between 3 and 4 numeric characters", validationErrors);
    }

}