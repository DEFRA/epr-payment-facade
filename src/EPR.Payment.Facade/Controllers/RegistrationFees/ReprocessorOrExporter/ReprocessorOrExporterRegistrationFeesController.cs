using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.RegistrationFees.ReProcessorOrExporter
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/reprocessororexporter")]
    [FeatureGate("EnableReprocessorOrExporterRegistrationFeesFeature")]
    public class ReprocessorOrExporterRegistrationFeesController : ControllerBase
    {
        private readonly ILogger<ReprocessorOrExporterRegistrationFeesController> _logger;
        private readonly IValidator<ReprocessorOrExporterRegistrationFeesRequestDto> _repoexpovalidator;

        public ReprocessorOrExporterRegistrationFeesController(
            ILogger<ReprocessorOrExporterRegistrationFeesController> logger,
            IValidator<ReprocessorOrExporterRegistrationFeesRequestDto> repoexpoValidator
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repoexpovalidator = repoexpoValidator ?? throw new ArgumentNullException(nameof(repoexpoValidator));
        }

        [HttpPost("registration-fee")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReprocessorOrExporterRegistrationFeesRequestDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Calculates the re-processor/exporter registration fees",
            Description = "Calculates the re-processor/exporter registration fees based on the provided request data."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the calculated registration fees for the re-processor/exporter.", typeof(ReprocessorOrExporterRegistrationFeesResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "If the request is valid but relevant resource data not found.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        [FeatureGate("EnableReprocessorOrExporterRegistrationFeesCalculation")]
        public async Task<IActionResult> CalculateFeesAsync([FromBody] ReprocessorOrExporterRegistrationFeesRequestDto reProcessorOrExporterFeesRequestDto, CancellationToken cancellationToken)
        {
            
            ValidationResult validationResult = _repoexpovalidator.Validate(reProcessorOrExporterFeesRequestDto);

            if (!validationResult.IsValid)
            {
                _logger.LogError(LogMessages.ValidationErrorOccured, nameof(CalculateFeesAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

                PreviousPaymentDetailResponseDto? previousPaymentDetail = null;

                var reProcessorOrExporterFResponse = new ReprocessorOrExporterRegistrationFeesResponseDto
                {
                    MaterialType = reProcessorOrExporterFeesRequestDto.MaterialType.ToString(),
                    RegistrationFee = 100.0m,
                    PreviousPaymentDetail = previousPaymentDetail,
                };

                return Ok(reProcessorOrExporterFResponse);
        }
    }
}