using Asp.Versioning;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FluentValidation;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;

namespace EPR.Payment.Facade.Controllers.Payments
{
    [ApiController]
    [Route("api/")]
    [FeatureGate("EnableOnlinePaymentsFeature")]
    public class OnlinePaymentsController : ControllerBase
    {
        private readonly IOnlinePaymentsService _onlinePaymentsService;
        private readonly ILogger<OnlinePaymentsController> _logger;
        private readonly string _errorUrl;

        public OnlinePaymentsController(IOnlinePaymentsService onlinePaymentsService, ILogger<OnlinePaymentsController> logger, IOptions<OnlinePaymentServiceOptions> onlinePaymentServiceOptions)
        {
            _onlinePaymentsService = onlinePaymentsService ?? throw new ArgumentNullException(nameof(onlinePaymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorUrl = onlinePaymentServiceOptions.Value.ErrorUrl ?? throw new ArgumentNullException(nameof(onlinePaymentServiceOptions));
        }

        [ApiExplorerSettings(GroupName = "v1")]
        [HttpPost("v1/online-payments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContentResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ContentResult))]
        [SwaggerOperation(
            Summary = "Initiates a new payment",
            Description = "Initiates a new payment with mandatory payment request data. Amount must be greater than 0. In case of an error, redirects to the error URL."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns an HTML content result with a redirect script.", typeof(ContentResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs, returns an HTML content result with a redirect script to the error URL.", typeof(ContentResult))]
        [FeatureGate("EnablePaymentInitiation")]
        public async Task<IActionResult> InitiateOnlinePayment([FromBody] OnlinePaymentRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ExceptionMessages.AmountMustBeGreaterThanZero,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var result = await _onlinePaymentsService.InitiateOnlinePaymentAsync(request, cancellationToken);

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
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(InitiateOnlinePayment));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccured, nameof(InitiateOnlinePayment));
                return new ContentResult
                {
                    Content = CreateHtmlContent(_errorUrl),
                    ContentType = "text/html",
                    StatusCode = StatusCodes.Status200OK
                };
            }
        }

        [HttpPost("v1/{externalPaymentId}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompleteOnlinePaymentResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ContentResult))]
        [SwaggerOperation(
            Summary = "Completes the payment process",
            Description = "Completes the payment process for the externalPaymentId requested. In case of an error, redirects to the error URL.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Payment completion process succeeded.", typeof(CompleteOnlinePaymentResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs, returns an HTML content result with a redirect script to the error URL.", typeof(ContentResult))]
        [FeatureGate("EnablePaymentCompletion")]
        public async Task<IActionResult> CompleteOnlinePayment(Guid externalPaymentId, CancellationToken cancellationToken)
        {
            if (externalPaymentId == Guid.Empty)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = "ExternalPaymentId cannot be empty.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var result = await _onlinePaymentsService.CompleteOnlinePaymentAsync(externalPaymentId, cancellationToken);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CompleteOnlinePayment));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccured, nameof(CompleteOnlinePayment));
                return new ContentResult
                {
                    Content = CreateHtmlContent(_errorUrl),
                    ContentType = "text/html",
                    StatusCode = StatusCodes.Status200OK
                };
            }
        }

        [ApiExplorerSettings(GroupName = "v2")]
        [HttpPost("v2/online-payments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContentResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ContentResult))]
        [SwaggerOperation(
           Summary = "Initiates a new payment",
           Description = "Initiates a new payment with mandatory payment request data. Amount must be greater than 0. In case of an error, redirects to the error URL."
       )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns an HTML content result with a redirect script.", typeof(ContentResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs, returns an HTML content result with a redirect script to the error URL.", typeof(ContentResult))]
        [FeatureGate("EnablePaymentInitiationV2")]
        public async Task<IActionResult> InitiateOnlinePaymentV2([FromBody] OnlinePaymentRequestV2Dto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ExceptionMessages.AmountMustBeGreaterThanZero,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var result = await _onlinePaymentsService.InitiateOnlinePaymentV2Async(request, cancellationToken);

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
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(InitiateOnlinePayment));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccured, nameof(InitiateOnlinePayment));
                return new ContentResult
                {
                    Content = CreateHtmlContent(_errorUrl),
                    ContentType = "text/html",
                    StatusCode = StatusCodes.Status200OK
                };
            }
        }

        private static string CreateHtmlContent(string nextUrl)
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
}