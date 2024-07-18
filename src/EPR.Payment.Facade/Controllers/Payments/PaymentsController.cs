﻿using Asp.Versioning;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    private readonly string _errorUrl;

    public PaymentsController(IPaymentsService paymentsService, ILogger<PaymentsController> logger, IOptions<PaymentServiceOptions> paymentServiceOptions)
    {
        _paymentsService = paymentsService ?? throw new ArgumentNullException(nameof(paymentsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorUrl = paymentServiceOptions.Value.ErrorUrl ?? throw new ArgumentNullException(nameof(paymentServiceOptions));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Initiates a new payment",
        Description = "Initiates a new payment with mandatory payment request data. In case of an error, redirects to the error URL."
    )]
    [SwaggerResponse(StatusCodes.Status302Found, "Redirects to the payment next URL.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs, redirects to the error URL.")]
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

            if (result.NextUrl == null)
            {
                _logger.LogError(LogMessages.NextUrlNull);
                return new ContentResult
                {
                    Content = CreateHtmlContent(_errorUrl),
                    ContentType = "text/html",
                    StatusCode = StatusCodes.Status200OK
                };
            }

            var htmlContent = CreateHtmlContent(result.NextUrl);

            return new ContentResult
            {
                Content = htmlContent,
                ContentType = "text/html",
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(InitiatePayment));
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.ErrorOccured, nameof(InitiatePayment));
            return new ContentResult
            {
                Content = CreateHtmlContent(_errorUrl),
                ContentType = "text/html",
                StatusCode = StatusCodes.Status200OK
            };
        }
    }

    [HttpPost("{govPayPaymentId}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Completes the payment process",
        Description = "Completes the payment process for the paymentId requested. In case of an error, redirects to the error URL."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment completion process succeeded.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs, redirects to the error URL.")]
    [FeatureGate("EnablePaymentCompletion")]
    public async Task<IActionResult> CompletePayment(string? govPayPaymentId, [FromBody] CompletePaymentRequestDto completeRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(govPayPaymentId))
        {
            return BadRequest(ExceptionMessages.GovPayPaymentIdNull);
        }

        try
        {
            await _paymentsService.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken);
            return Ok();
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CompletePayment));
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.ErrorOccured, nameof(CompletePayment));
            return new ContentResult
            {
                Content = CreateHtmlContent(_errorUrl),
                ContentType = "text/html",
                StatusCode = StatusCodes.Status200OK
            };
        }
    }

    private string CreateHtmlContent(string nextUrl)
    {
        return $@"
    <!DOCTYPE html>
    <html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Redirecting...</title>
        <script>
            window.location.href = '{nextUrl}';
        </script>
    </head>
    <body>
        Redirecting to payment page...
    </body>
    </html>";
    }
}
