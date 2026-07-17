using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Services.RegistrationFees.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.RegistrationFees
{
    [ApiController]
    [Route("api/")]
    public class SubmissionPeriodsController : ControllerBase
    {
        private readonly ISubmissionPeriodsService _submissionPeriodsService;
        private readonly ILogger<SubmissionPeriodsController> _logger;

        public SubmissionPeriodsController(
            ISubmissionPeriodsService submissionPeriodsService,
            ILogger<SubmissionPeriodsController> logger)
        {
            _submissionPeriodsService = submissionPeriodsService ?? throw new ArgumentNullException(nameof(submissionPeriodsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [ApiExplorerSettings(GroupName = "v1")]
        [HttpGet("v1/submission-periods")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<SubmissionPeriodResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Lists all submission periods",
            Description = "Returns the full set of registration submission periods (one row per WindowType and RegistrationYear).")]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of submission periods.", typeof(IReadOnlyList<SubmissionPeriodResponseDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        public async Task<IActionResult> GetSubmissionPeriods(CancellationToken cancellationToken)
        {
            try
            {
                var periods = await _submissionPeriodsService.GetAllAsync(cancellationToken);
                return Ok(periods);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorRetrievingSubmissionPeriods);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Service Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                });
            }
        }
    }
}
