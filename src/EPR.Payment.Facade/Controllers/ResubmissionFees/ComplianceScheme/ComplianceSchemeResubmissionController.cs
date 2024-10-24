using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.ResubmissionFees.ComplianceScheme
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/compliance-scheme/resubmission-fees")]
    [FeatureGate("EnableResubmissionComplianceSchemeFeature")]
    public class ComplianceSchemeResubmissionController : ControllerBase
    {
        private readonly IValidator<ComplianceSchemeResubmissionFeeRequestDto> _resubmissionValidator;
        private readonly IComplianceSchemeResubmissionFeesService _resubmissionFeesService;
        private readonly ILogger<ComplianceSchemeResubmissionController> _logger;

        public ComplianceSchemeResubmissionController(
            IComplianceSchemeResubmissionFeesService resubmissionFeesService,
            ILogger<ComplianceSchemeResubmissionController> logger,
            IValidator<ComplianceSchemeResubmissionFeeRequestDto> resubmissionValidator)
        {
            _resubmissionValidator = resubmissionValidator ?? throw new ArgumentNullException(nameof(resubmissionValidator));
            _resubmissionFeesService = resubmissionFeesService ?? throw new ArgumentNullException(nameof(resubmissionFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [FeatureGate("EnableResubmissionFeesCalculation")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesResponseType(typeof(ComplianceSchemeResubmissionFeeResult), 200)]
        [SwaggerOperation(
            Summary = "Compliance Scheme resubmission fee calculation",
            Description = "Calculates the compliance scheme resubmission fee based on the provided request details.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        public async Task<IActionResult> CalculateResubmissionFeeAsync([FromBody] ComplianceSchemeResubmissionFeeRequestDto request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await _resubmissionValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogError(LogMessages.ValidationErrorOccured, nameof(CalculateResubmissionFeeAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var resubmissionFeesResponse = await _resubmissionFeesService.CalculateResubmissionFeeAsync(request, cancellationToken);
                return Ok(resubmissionFeesResponse);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateResubmissionFeeAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingComplianceSchemeFees, nameof(CalculateResubmissionFeeAsync));
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