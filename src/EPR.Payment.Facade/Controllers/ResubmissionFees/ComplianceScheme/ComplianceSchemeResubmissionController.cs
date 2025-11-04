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
    [ApiController]
    [Route("api")]
    [FeatureGate("EnableResubmissionComplianceSchemeFeature")]
    public class ComplianceSchemeResubmissionController : ControllerBase
    {
        private readonly IValidator<ComplianceSchemeResubmissionFeeRequestDto> _resubmissionValidator;
        private readonly IValidator<ComplianceSchemeResubmissionFeeRequestV2Dto> _resubmissionValidatorV2;
        private readonly IComplianceSchemeResubmissionFeesService _resubmissionFeesService;
        private readonly ILogger<ComplianceSchemeResubmissionController> _logger;

        public ComplianceSchemeResubmissionController(
            IComplianceSchemeResubmissionFeesService resubmissionFeesService,
            ILogger<ComplianceSchemeResubmissionController> logger,
            IValidator<ComplianceSchemeResubmissionFeeRequestDto> resubmissionValidator,
            IValidator<ComplianceSchemeResubmissionFeeRequestV2Dto> resubmissionValidatorV2)
        {
            _resubmissionValidator = resubmissionValidator ?? throw new ArgumentNullException(nameof(resubmissionValidator));
            _resubmissionValidatorV2 = resubmissionValidatorV2 ?? throw new ArgumentNullException(nameof(resubmissionValidatorV2));
            _resubmissionFeesService = resubmissionFeesService ?? throw new ArgumentNullException(nameof(resubmissionFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [ApiExplorerSettings(GroupName = "v1")]
        [HttpPost("v1/compliance-scheme/resubmission-fees")]
        [FeatureGate("EnableResubmissionFeesCalculation")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesResponseType(typeof(ComplianceSchemeResubmissionFeeResponse), 200)]
        [SwaggerOperation(
            Summary = "Compliance Scheme resubmission fee calculation",
            Description = "Calculates the compliance scheme resubmission fee based on the provided request details.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        public async Task<IActionResult> CalculateResubmissionFeeAsync([FromBody] ComplianceSchemeResubmissionFeeRequestDto complianceSchemeResubmissionFeeRequestDto, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await _resubmissionValidator.ValidateAsync(complianceSchemeResubmissionFeeRequestDto, cancellationToken);
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
                var resubmissionFeesResponse = await _resubmissionFeesService.CalculateResubmissionFeeAsync(complianceSchemeResubmissionFeeRequestDto, cancellationToken);
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

        [ApiExplorerSettings(GroupName = "v2")]
        [HttpPost("v2/compliance-scheme/resubmission-fees")]
        [FeatureGate("EnableResubmissionFeesCalculation")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesResponseType(typeof(ComplianceSchemeResubmissionFeeResponse), 200)]
        [SwaggerOperation(
            Summary = "Compliance Scheme resubmission fee calculation",
            Description = "Calculates the compliance scheme resubmission fee based on the provided request details.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        public async Task<IActionResult> CalculateResubmissionFeeAsyncV2([FromBody] ComplianceSchemeResubmissionFeeRequestV2Dto complianceSchemeResubmissionFeeRequestDto, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await _resubmissionValidatorV2.ValidateAsync(complianceSchemeResubmissionFeeRequestDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogError(LogMessages.ValidationErrorOccured, nameof(CalculateResubmissionFeeAsyncV2));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var resubmissionFeesResponse = await _resubmissionFeesService.CalculateResubmissionFeeAsync(complianceSchemeResubmissionFeeRequestDto, cancellationToken);
                return Ok(resubmissionFeesResponse);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateResubmissionFeeAsyncV2));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingComplianceSchemeFees, nameof(CalculateResubmissionFeeAsyncV2));
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