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
            Currency = "GBP"
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
}