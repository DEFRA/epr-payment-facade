using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Services.ResubmissionFees.Producer.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.ResubmissionFees.Producer
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/producer")]
    [FeatureGate("EnableProducersResubmissionFeesFeature")]
    public class ProducerResubmissionController : ControllerBase
    {
        private readonly IValidator<RegulatorDto> _resubmissionValidator;
        private readonly IProducerResubmissionFeesService _producerResubmissionFeesService;
        private readonly ILogger<ProducerResubmissionController> _logger;

        public ProducerResubmissionController(
            IProducerResubmissionFeesService producerResubmissionFeesService,
            ILogger<ProducerResubmissionController> logger,
            IValidator<RegulatorDto> resubmissionValidator
            )
        {
            _resubmissionValidator = resubmissionValidator ?? throw new ArgumentNullException(nameof(resubmissionValidator));
            _producerResubmissionFeesService = producerResubmissionFeesService ?? throw new ArgumentNullException(nameof(producerResubmissionFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("resubmission-fee")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Producer registration resubmission fee by regulator",
            Description = "Return producer registration resubmission fee by regulator."
        )]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        [FeatureGate("EnableProducerResubmissionFee")]
        public async Task<IActionResult> GetResubmissionFeeAsync([FromQuery] RegulatorDto request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = _resubmissionValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                _logger.LogError(LogMessages.ValidationErrorOccured, nameof(GetResubmissionFeeAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var registrationFeesResponse = await _producerResubmissionFeesService.GetResubmissionFeeAsync(request, cancellationToken);
                return Ok(registrationFeesResponse);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(GetResubmissionFeeAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingProducerFees, nameof(GetResubmissionFeeAsync));
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected Error",
                    Detail = ExceptionMessages.UnexpectedErrorCalculatingFees,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
