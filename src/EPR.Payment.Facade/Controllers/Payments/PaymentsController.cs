using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
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
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Initiates a new payment", Description = "Initiates a new payment with mandatory payment request data.")]
    [SwaggerResponse(StatusCodes.Status302Found, "Redirects to the payment next URL.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
    [FeatureGate("EnablePaymentInitiation")]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _paymentsService.InitiatePaymentAsync(request, cancellationToken);

            // Return a simple HTML page with JavaScript to perform the redirection
            var htmlContent = $@"
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Redirecting...</title>
            <script>
                window.location.href = '{result.NextUrl}';
            </script>
        </head>
        <body>
            Redirecting to payment page...
        </body>
        </html>";

            return new ContentResult
            {
                Content = htmlContent,
                ContentType = "text/html",
                StatusCode = StatusCodes.Status200OK
            };
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
    public async Task<IActionResult> CompletePayment(string? govPayPaymentId, [FromBody] CompletePaymentRequestDto completeRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(govPayPaymentId))
        {
            return BadRequest("GovPayPaymentId cannot be null or empty");
        }

        try
        {
            await _paymentsService.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken);
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
