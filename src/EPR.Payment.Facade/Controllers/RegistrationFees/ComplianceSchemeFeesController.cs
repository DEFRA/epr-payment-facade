using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.RegistrationFees
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/compliance-scheme")]
    [FeatureGate("EnableComplianceSchemeFeesFeature")]
    public class ComplianceSchemeFeesController : ControllerBase
    {
        private readonly IComplianceSchemeFeesService _complianceSchemeFeesService;
        private readonly ILogger<ComplianceSchemeFeesController> _logger;
        private readonly IValidator<RegulatorDto> _complianceSchemeValidator;

        public ComplianceSchemeFeesController(
            IComplianceSchemeFeesService complianceSchemeFeesService,
            ILogger<ComplianceSchemeFeesController> logger,
            IValidator<RegulatorDto> complianceSchemeValidator)
        {
            _complianceSchemeFeesService = complianceSchemeFeesService ?? throw new ArgumentNullException(nameof(complianceSchemeFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _complianceSchemeValidator = complianceSchemeValidator ?? throw new ArgumentNullException(nameof(complianceSchemeValidator));
        }

        [HttpGet("registration-fee")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(decimal))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Retrieves the compliance scheme base fee",
            Description = "Retrieves the compliance scheme base fee based on the specified regulator."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the base fee for the compliance scheme.", typeof(decimal))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        [FeatureGate("EnableComplianceSchemeBaseFees")]
        public async Task<IActionResult> GetBaseFeeAsync([FromQuery] RegulatorDto request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = _complianceSchemeValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                _logger.LogError(LogMessages.ValidationErrorOccured, nameof(GetBaseFeeAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var baseFeeResponse = await _complianceSchemeFeesService.GetComplianceSchemeBaseFeeAsync(request, cancellationToken);
                return Ok(baseFeeResponse);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(GetBaseFeeAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingComplianceSchemeFees, nameof(GetBaseFeeAsync));
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Service Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingComplianceSchemeFees, nameof(GetBaseFeeAsync));
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected Error",
                    Detail = ExceptionMessages.UnexpectedErrorRetrievingComplianceSchemeBaseFee,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
