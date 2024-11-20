namespace PaymentGateway.Api.Models.Responses;

    public class BankSimulatorResponse
    {
        public bool authorized { get; set; }
        public string authorization_code { get; set; } = default!;
        public string errorMessage { get; set; } = default!;
    }