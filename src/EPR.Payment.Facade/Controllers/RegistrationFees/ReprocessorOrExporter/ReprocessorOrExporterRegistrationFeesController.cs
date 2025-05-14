using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.RegistrationFees.ReProcessorOrExporter
{
    [AllowAnonymous]
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/reprocessororexporter")]
    [FeatureGate("EnableReprocessorOrExporterRegistrationFeesFeature")]
    public class ReprocessorOrExporterRegistrationFeesController : ControllerBase
    {
        [HttpPost("registration-fee")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReprocessorOrExporterRegistrationFeesResponseDto))]
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
        [FeatureGate("EnableReprocessorOrExporterRegistrationFeesFeature")]
        public async Task<IActionResult> CalculateFeesAsync([FromBody] ReprocessorOrExporterRegistrationFeesRequestDto reProcessorOrExporterFeesRequestDto, CancellationToken cancellationToken)
        {
            return await Task.FromResult(Ok(new ReprocessorOrExporterRegistrationFeesResponseDto
            {
                MaterialType = "Plastic",
                RegistrationFee = 100.0m
            }));
        }
    }
}
