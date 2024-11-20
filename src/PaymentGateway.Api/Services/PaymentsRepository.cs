using System.Text.Json;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    public List<PostPaymentResponse> Payments = new();
    private readonly BankSimulatorRepository _bankSimulatorRepository;

    public PaymentsRepository(BankSimulatorRepository bankSimulatorRepository)
    {
        _bankSimulatorRepository = bankSimulatorRepository;
    }

    public void Add(PostPaymentResponse payment)
    {
        Payments.Add(payment);
    }

    public PostPaymentResponse Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id)!;
    }

    public async Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest request)
    {
        //check if card is valid with a future date
        if (request.ExpiryYear < DateTime.UtcNow.Year ||
            (request.ExpiryYear == DateTime.UtcNow.Year && request.ExpiryMonth < DateTime.UtcNow.Month))
        {
            throw new ArgumentException("Card has expired... Please use a valid card.");
        }

        // Call bank simulator API
        var status = await _bankSimulatorRepository.ProcessPaymentAsync(request);
        if (status == null)
        {
            throw new Exception("Error processing payment: No response from simulator.");
        }

        //var paymentStatus = status.authorized ? PaymentStatus.Authorized : PaymentStatus.Rejected;

        Console.WriteLine($"Response received from bank api; Authorized status: {JsonSerializer.Serialize(status.authorized)}, Authorization code: {JsonSerializer.Serialize(status.authorization_code)}");
        
        var isAuthorized = JsonSerializer.Serialize(status.authorized);
        
        PaymentStatus paymentStatus;
        if (isAuthorized == "true")
        {
            paymentStatus = PaymentStatus.Authorized;
        }
        else
        {
            paymentStatus = PaymentStatus.Rejected;
        }

        // Store payment details
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = paymentStatus.ToString(),
            CardNumberLastFour = request.CardNumberLastFour,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };

        Payments.Add(payment);
        return payment;
    }
}