using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    private readonly PaymentsRepository _paymentsRepository;
    private readonly BankSimulatorRepository _bankSimulatorRepository;

    public PaymentsControllerTests()
    {
        // substitute for bankSimulatorRepository
        _bankSimulatorRepository = Substitute.For<BankSimulatorRepository>(default(HttpClient));

        _paymentsRepository = new PaymentsRepository(_bankSimulatorRepository);
    }

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999).ToString(),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository(_bankSimulatorRepository);
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPaymentReturnsRejectedWhenTransactionIsRejected()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumberLastFour = "8112",
            ExpiryMonth = 1,
            ExpiryYear = 2026,
            Currency = "USD",
            Amount = 60000,
            Cvv = "456"
        };

        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = "Rejected",
            Currency = "USD",
            Amount = request.Amount,
            CardNumberLastFour = request.CardNumberLastFour
        };

        var paymentsRepository = new PaymentsRepository(_bankSimulatorRepository);
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal("Rejected", paymentResponse?.Status);
    }

    [Fact]
    public async Task ProcessPaymentReturnsBadRequestWhenInvalidPayload()
    {
        // Arrange
        var invalidRequest = new PostPaymentRequest
        {
            CardNumberLastFour = "12",
            ExpiryMonth = 13,
            ExpiryYear = 2023,
            Currency = "YEN",
            Amount = -10, 
            Cvv = "B4K3" 
        };

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", invalidRequest);
        var errorResponse = await response.Content.ReadFromJsonAsync<BankSimulatorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errorResponse);
    }

}