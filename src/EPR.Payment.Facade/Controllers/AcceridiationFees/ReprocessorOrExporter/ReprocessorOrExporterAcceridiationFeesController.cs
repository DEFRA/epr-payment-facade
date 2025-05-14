using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request.AcceridiationFees.ReprocessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.AcceridiationFees.ReprocessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.AcceridiationFees.ReProcessorOrExporter
{
    [AllowAnonymous]
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/reprocessororexporter")]
    [FeatureGate("EnableReprocessorOrExporterAcceridiationFeesFeature")]
    public class ReprocessorOrExporterAcceridiationFeesController : ControllerBase
    {
        [HttpPost("acceridiation-fee")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReprocessorOrExporterAcceridiationFeesResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Calculates the re-processor/exporter acceridiation fees",
            Description = "Calculates the re-processor/exporter acceridiation fees based on the provided request data."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the calculated acceridiation fees for the re-processor/exporter.", typeof(ReprocessorOrExporterAcceridiationFeesResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "If the request is valid but relevant resource data not found.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        [FeatureGate("EnableReprocessorOrExporterAcceridiationFeesFeature")]
        public async Task<IActionResult> CalculateFeesAsync([FromBody] ReprocessorOrExporterAcceridiationFeesRequestDto reProcessorOrExporterFeesRequestDto, CancellationToken cancellationToken)
        {
            return await Task.FromResult(Ok(new ReprocessorOrExporterAcceridiationFeesResponseDto
            {
                OverseasChargePerSite = 0,
                TotalOverseasSitesCharges = 0,
                TonnageBandCharge = 200.0m,
                TotalAcceridiationFees = 200.0m,
                PreviousPaymentDetail = new PreviousPaymentDetailResponseDto()
                {
                    PaymentDate = DateTime.UtcNow,
                    PaymentAmount = 200.0m,
                    PaymentMethod = "Cheque",
                    PaymentMode = "Offline"
                }
            }));
        }
    }
}
