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
    [ApiController]
    
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

        [Route("api/v1/offline-payments")]
        [HttpPost()]
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
        public async Task<IActionResult> OfflinePaymentV1([FromBody] OfflinePaymentRequestDto offlinePaymentRequestDto, CancellationToken cancellationToken)
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
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(OfflinePaymentV1));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileInsertingOfflinePayment, nameof(OfflinePaymentV1));
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected Error",
                    Detail = ExceptionMessages.UnexpectedErrorInsertingOfflinePayment,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [Route("api/v2/offline-payments")]
        [HttpPost()]
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
        public async Task<IActionResult> OfflinePaymentV2([FromBody] OfflinePaymentV2RequestDto offlinePaymentRequestDto, CancellationToken cancellationToken)
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
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(OfflinePaymentV2));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileInsertingOfflinePayment, nameof(OfflinePaymentV2));
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