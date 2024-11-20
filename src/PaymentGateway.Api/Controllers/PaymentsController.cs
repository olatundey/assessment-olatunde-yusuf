using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;

using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;

    public PaymentsController(PaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);
        if (payment == null) 
        {
            return NotFound(new { Error = "Payment not found." });
        }
        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessPaymentAsync([FromBody] PostPaymentRequest request)
    {
        try
        {
            var payment = await _paymentsRepository.ProcessPaymentAsync(request);
            return new OkObjectResult(payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}