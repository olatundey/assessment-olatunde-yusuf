using System.Text.Json;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class BankSimulatorRepository
{
    private readonly HttpClient _httpClient;

    public BankSimulatorRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BankSimulatorResponse> ProcessPaymentAsync(PostPaymentRequest request)
    {
        var requestBankApi = new
        {
            card_number = $"222240534324{request.CardNumberLastFour}",
            expiry_date = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            currency = request.Currency,
            amount = request.Amount,
            cvv = request.Cvv
        };

        try
        {
            Console.WriteLine($"Sending payment request: {JsonSerializer.Serialize(request)}");
            var response = await _httpClient.PostAsJsonAsync("http://localhost:8080/payments", requestBankApi);
            Console.WriteLine($"Bank simulator response: {response.StatusCode}, Content: {response}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed: {response.StatusCode}, Bank simulator error response: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<BankSimulatorResponse>();
            return result!;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing payment: {ex.Message}", ex);
        }
    }
}