using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests
{
    public class PostPaymentRequest
    {
        [Required, StringLength(4, MinimumLength = 4), RegularExpression(@"^\d{4}$", ErrorMessage = "Card last four numbers must be exactly 4 digits.")]
        public string? CardNumberLastFour { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Expiry Month must be between 1 and 12.")]
        public int ExpiryMonth { get; set; }

        [Required]
        [Range(2024, 2999, ErrorMessage = "Expiry Year must be between {1} and {2}.")]
        public int ExpiryYear { get; set; }

        [Required, RegularExpression("^(GBP|EUR|USD)$", ErrorMessage = "Currency must be one of the following: GBP, EUR, USD.")]
        public string? Currency { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public int Amount { get; set; }

        [Required, StringLength(4, MinimumLength = 3), RegularExpression(@"^\d{3,4}$", ErrorMessage = "Cvv must be 3 or 4 digits.")]
        public string? Cvv { get; set; }
    }
}