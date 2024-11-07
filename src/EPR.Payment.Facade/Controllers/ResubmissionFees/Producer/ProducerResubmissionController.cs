using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
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
        private readonly IValidator<ProducerResubmissionFeeRequestDto> _resubmissionValidator;
        private readonly IProducerResubmissionFeesService _producerResubmissionFeesService;
        private readonly ILogger<ProducerResubmissionController> _logger;

        public ProducerResubmissionController(
            IProducerResubmissionFeesService producerResubmissionFeesService,
            ILogger<ProducerResubmissionController> logger,
            IValidator<ProducerResubmissionFeeRequestDto> resubmissionValidator)
        {
            _resubmissionValidator = resubmissionValidator ?? throw new ArgumentNullException(nameof(resubmissionValidator));
            _producerResubmissionFeesService = producerResubmissionFeesService ?? throw new ArgumentNullException(nameof(producerResubmissionFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("resubmission-fee")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProducerResubmissionFeeResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Calculate producer resubmission fee",
            Description = "Calculates the resubmission fee for a producer based on provided request details."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the calculated resubmission fees", typeof(ProducerResubmissionFeeResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request due to validation errors or invalid input", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error occurred while calculating the fee", typeof(ProblemDetails))]
        [FeatureGate("EnableProducerResubmissionFee")]
        public async Task<IActionResult> GetResubmissionFeeAsync([FromBody] ProducerResubmissionFeeRequestDto producerResubmissionFeeRequestDto, CancellationToken cancellationToken)
        {
            // Validate the request
            ValidationResult validationResult = _resubmissionValidator.Validate(producerResubmissionFeeRequestDto);
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
                var response = await _producerResubmissionFeesService.GetResubmissionFeeAsync(producerResubmissionFeeRequestDto, cancellationToken);
                return Ok(response);
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
