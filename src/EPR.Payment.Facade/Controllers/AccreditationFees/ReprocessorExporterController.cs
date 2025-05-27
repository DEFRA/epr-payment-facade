using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace EPR.Payment.Facade.Controllers.AccreditationFees
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/reprocessorexporter")]
    [FeatureGate("EnableReprocessorExporterAccreditationFeesFeature")]
    public class ReprocessorExporterController: ControllerBase
    {
        private readonly ILogger<ReprocessorExporterController> _logger;
        private readonly IValidator<AccreditationFeesRequestDto> _accreditationFeesRequestvalidator;

        public ReprocessorExporterController(
           ILogger<ReprocessorExporterController> logger,
           IValidator<AccreditationFeesRequestDto> accreditationFeesRequestvalidator
           )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accreditationFeesRequestvalidator = accreditationFeesRequestvalidator ?? throw new ArgumentNullException(nameof(accreditationFeesRequestvalidator));
        }

        [HttpPost("accreditation-fee")]
        [ProducesResponseType(typeof(ProducerResubmissionFeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Calculates the accreditation fee for a exporter or reprocessor",
            Description = "Calculates the accreditation fee for a exporter or reprocessor based on provided request details."
        )]
        [FeatureGate("EnableReprocessorExporterAccreditationFeesCalculation")]
        public async Task<IActionResult> GetAccreditationFee([FromBody] AccreditationFeesRequestDto request,
            CancellationToken cancellationToken)
        {
            var validationResult = _accreditationFeesRequestvalidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var accreditationFeesResponse = new AccreditationFeesResponseDto
            {
                OverseasSiteChargePerSite = 75.00m,
                TotalOverseasSitesCharges = 225.00m,
                TonnageBandCharge = 310.00m,
                // TotalAccreditationFees is computed automatically
                PreviousPaymentDetail = new AccreditationFeesPreviousPayment
                {
                    PaymentMode   = "offline",
                    PaymentMethod = "bank transfer",
                    PaymentAmount = 200.00m,
                    PaymentDate   = new DateTime(2024, 11, 15)
                }
            };

            return Ok(accreditationFeesResponse);
        }
    }
}
