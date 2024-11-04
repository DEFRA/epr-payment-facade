using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
        private readonly IValidator<OfflinePaymentRequestDto> _offlinePaymentRequestValidator;

        public OfflinePaymentsController(IOfflinePaymentsService offlinePaymentsService,
            ILogger<OfflinePaymentsController> logger,
            IValidator<OfflinePaymentRequestDto> offlinePaymentRequestValidator)
        {
            _offlinePaymentsService = offlinePaymentsService ?? throw new ArgumentNullException(nameof(offlinePaymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offlinePaymentRequestValidator = offlinePaymentRequestValidator ?? throw new ArgumentNullException(nameof(offlinePaymentRequestValidator));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(NoContentResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ContentResult))]
        [SwaggerOperation(
            Summary = "Saves a new offlinepayment",
            Description = "Initiates a new payment with mandatory payment request data.  "
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, $"Returns No Content", typeof(NoContentResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ContentResult))]
        [FeatureGate("EnableOfflinePayment")]
        public async Task<IActionResult> OfflinePayment([FromBody] OfflinePaymentRequestDto offlinePaymentRequestDto, CancellationToken cancellationToken)
        {
            var validatorResult = _offlinePaymentRequestValidator.Validate(offlinePaymentRequestDto);

            if (!validatorResult.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validatorResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                await _offlinePaymentsService.OfflinePaymentAsync(offlinePaymentRequestDto, cancellationToken);

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
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileInsertingOfflinePayment, nameof(OfflinePayment));
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected Error",
                    Detail = ExceptionMessages.UnexpectedErrorInsertingOfflinePayment,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}