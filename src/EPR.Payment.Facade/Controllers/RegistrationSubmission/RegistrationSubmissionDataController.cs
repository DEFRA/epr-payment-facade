using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Services.RegistrationSubmission.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.RegistrationSubmission
{
    [ApiController]
    [Route("api/")]
    public class RegistrationSubmissionDataController : ControllerBase
    {
        private readonly IRegistrationSubmissionDataService _service;
        private readonly ILogger<RegistrationSubmissionDataController> _logger;

        public RegistrationSubmissionDataController(
            IRegistrationSubmissionDataService service,
            ILogger<RegistrationSubmissionDataController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
