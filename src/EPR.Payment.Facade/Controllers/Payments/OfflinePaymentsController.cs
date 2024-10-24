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

namespace EPR.Payment.Facade.Controllers.Payments
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/offline-payments")]
    [FeatureGate("EnableOfflinePaymentsFeature")]
    public class OfflinePaymentsController : ControllerBase
    {
        private readonly IOfflinePaymentsService _offlinePaymentsService;
        private readonly ILogger<OfflinePaymentsController> _logger;
        private readonly string _errorUrl;

        public OfflinePaymentsController(IOfflinePaymentsService offlinePaymentsService, ILogger<OfflinePaymentsController> logger, IOptions<OfflinePaymentServiceOptions> offlinePaymentServiceOptions)
        {
            _offlinePaymentsService = offlinePaymentsService ?? throw new ArgumentNullException(nameof(offlinePaymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorUrl = offlinePaymentServiceOptions.Value.Description ?? throw new ArgumentNullException(nameof(offlinePaymentServiceOptions));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContentResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ContentResult))]
        [SwaggerOperation(
            Summary = "Saves a new offlinepayment",
            Description = "Initiates a new payment with mandatory payment request data.  "
        )]
        [SwaggerResponse(StatusCodes.Status200OK, $"Returns {nameof(OfflinePaymentResponseDto)}", typeof(ContentResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ContentResult))]
        [FeatureGate("EnableOfflinePayment")]
        public async Task<IActionResult> OfflinePayment([FromBody] OfflinePaymentRequestDto request, CancellationToken cancellationToken)
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
                await _offlinePaymentsService.OfflinePaymentAsync(request, cancellationToken);

                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(OfflinePayment));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccured, nameof(OfflinePayment));
                return BadRequest(new ProblemDetails
                {
                    Title = "Internal Server Error: Unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}