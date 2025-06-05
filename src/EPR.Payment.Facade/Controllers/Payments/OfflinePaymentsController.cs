using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.Payments{
    
    [ApiController]
    [Route("api/")]
    [FeatureGate("EnableOfflinePaymentsFeature")]
    public class OfflinePaymentsController : ControllerBase
    {
        private readonly IOfflinePaymentsService _offlinePaymentsService;
        private readonly ILogger<OfflinePaymentsController> _logger;
        private readonly IValidator<OfflinePaymentRequestDto> _offlinePaymentRequestValidator;
        private readonly IValidator<OfflinePaymentRequestV2Dto> _offlinePaymentRequestV2Validator;

        public OfflinePaymentsController(IOfflinePaymentsService offlinePaymentsService,
            ILogger<OfflinePaymentsController> logger,
            IValidator<OfflinePaymentRequestDto> offlinePaymentRequestValidator,
            IValidator<OfflinePaymentRequestV2Dto> offlinePaymentRequestV2Validator
            )
        {
            _offlinePaymentsService = offlinePaymentsService ?? throw new ArgumentNullException(nameof(offlinePaymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offlinePaymentRequestValidator = offlinePaymentRequestValidator ?? throw new ArgumentNullException(nameof(offlinePaymentRequestValidator));
            _offlinePaymentRequestV2Validator = offlinePaymentRequestV2Validator ?? throw new ArgumentNullException(nameof(offlinePaymentRequestV2Validator));
        }

        [ApiExplorerSettings(GroupName = "v1")]
        [HttpPost("v1/offline-payments")]
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

            return await ExecuteWithErrorHanding(() => _offlinePaymentsService.OfflinePaymentAsync(offlinePaymentRequestDto, cancellationToken));
        }

        [ApiExplorerSettings(GroupName = "v2")]
        [HttpPost("v2/offline-payments")]
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
        public async Task<IActionResult> OfflinePaymentV2([FromBody] OfflinePaymentRequestV2Dto offlinePaymentRequestV2Dto, CancellationToken cancellationToken)
        {
            var validatorResult = _offlinePaymentRequestV2Validator.Validate(offlinePaymentRequestV2Dto);

            if (!validatorResult.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validatorResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return await ExecuteWithErrorHanding(() => _offlinePaymentsService.OfflinePaymentAsync(offlinePaymentRequestV2Dto, cancellationToken));
        }

        private async Task<ActionResult> ExecuteWithErrorHanding(Func<Task> asyncAction)
        {
            try
            {
                await asyncAction();

                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(asyncAction));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileInsertingOfflinePayment, nameof(asyncAction));
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