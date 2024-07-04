using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/payments")]
[FeatureGate("EnablePaymentsFeature")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentsService _paymentsService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentsService paymentsService, ILogger<PaymentsController> logger)
    {
        _paymentsService = paymentsService ?? throw new ArgumentNullException(nameof(paymentsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Initiates a new payment", Description = "Initiates a new payment with mandatory payment request data. <br>" +
    "Return_url input parameter is the URL that Gov Pay will return back to when the payment journey is complete. <br>" +
    "The return_url parameter in the response object is the initial page in the Gov Pay journey.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Returns the created payment response.", typeof(PaymentResponseDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
    [FeatureGate("EnablePaymentInitiation")]
    public async Task<ActionResult<PaymentResponseDto>> InitiatePayment([FromBody] PaymentRequestDto request)
    {
        var context = new ValidationContext(request, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, context, results, true);

        if (!isValid)
        {
            var errorMessage = string.Join(", ", results.Select(r => r.ErrorMessage));
            _logger.LogError("Validation failed: {ValidationErrors}", errorMessage);
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var result = await _paymentsService.InitiatePaymentAsync(request);
            return CreatedAtAction(nameof(CompletePayment), new { paymentId = result.PaymentId }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while processing InitiatePayment request");
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing InitiatePayment request");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpPost("{govPayPaymentId}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Completes the payment process", Description = "Completes the payment process for the paymentId requested.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment completion process succeeded.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.")]
    [FeatureGate("EnablePaymentCompletion")]
    public async Task<IActionResult> CompletePayment(string govPayPaymentId, [FromBody] CompletePaymentRequestDto completeRequest)
    {
        if (string.IsNullOrEmpty(govPayPaymentId))
        {
            return BadRequest("GovPayPaymentId cannot be null or empty");
        }

        try
        {
            await _paymentsService.CompletePaymentAsync(govPayPaymentId, completeRequest);
            return Ok();
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while processing CompletePayment request");
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing CompletePayment request");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
