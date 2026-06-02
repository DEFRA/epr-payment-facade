using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Services.RegistrationSubmission.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.RegistrationSubmission
{
    [ApiController]
    [Route("api/")]
    [FeatureGate("EnableRegistrationSubmissionDataFeature")]
    public class RegistrationSubmissionDataController : ControllerBase
    {
        private readonly IRegistrationSubmissionDataService _service;
        private readonly ILogger<RegistrationSubmissionDataController> _logger;
        private readonly IValidator<CreateRegistrationSubmissionDataRequest> _validator;

        public RegistrationSubmissionDataController(
            IRegistrationSubmissionDataService service,
            ILogger<RegistrationSubmissionDataController> logger,
            IValidator<CreateRegistrationSubmissionDataRequest> validator)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        [ApiExplorerSettings(GroupName = "v1")]
        [HttpPost("v1/registration-submission-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Creates the registration submission data snapshot",
            Description = "Forwards the request to the payment service which extracts producer/subsidiary data from the registration CSV."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the Id of the created RegistrationSubmissionData row.", typeof(Guid))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        [FeatureGate("EnableRegistrationSubmissionData")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateRegistrationSubmissionDataRequest request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                _logger.LogError(LogMessages.ValidationErrorOccured, nameof(CreateAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest,
                });
            }

            try
            {
                var id = await _service.CreateAsync(request, cancellationToken);
                return Ok(id);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CreateAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorCreatingRegistrationSubmissionData);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Service Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCreatingRegistrationSubmissionData);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected Error",
                    Detail = ExceptionMessages.UnexpectedErrorCreatingRegistrationSubmissionData,
                    Status = StatusCodes.Status500InternalServerError,
                });
            }
        }

        [ApiExplorerSettings(GroupName = "v1")]
        [HttpGet("v1/registration-submission-data/{submissionId:guid}/fee-calculation-details")]
        [ProducesResponseType(typeof(IReadOnlyList<RegistrationFeeCalculationDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Gets registration fee calculation details from the snapshot tables",
            Description = "Returns one row per producer for the latest registration submission snapshot. 404 when no snapshot exists for the SubmissionId."
        )]
        [FeatureGate("EnableRegistrationSubmissionData")]
        public async Task<IActionResult> GetFeeCalculationDetails(Guid submissionId, CancellationToken cancellationToken)
        {
            if (submissionId == Guid.Empty)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = "SubmissionId is required.",
                    Status = StatusCodes.Status400BadRequest,
                });
            }

            try
            {
                var details = await _service.GetFeeCalculationDetailsAsync(submissionId, cancellationToken);
                if (details is null)
                {
                    return NotFound();
                }

                return Ok(details);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorRetrievingRegistrationFeeCalculationDetails);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Service Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorRetrievingRegistrationFeeCalculationDetails);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected Error",
                    Detail = ExceptionMessages.ErrorRetrievingRegistrationFeeCalculationDetails,
                    Status = StatusCodes.Status500InternalServerError,
                });
            }
        }
    }
}
