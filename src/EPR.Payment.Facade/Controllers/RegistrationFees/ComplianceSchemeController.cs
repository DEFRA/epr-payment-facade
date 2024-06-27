using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Controllers.RegistrationFees
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/complianceschemes")]
    [FeatureGate("EnableComplianceSchemeFeature")]
    public class ComplianceSchemeController : ControllerBase
    {
        private readonly IFeesService _feesService;

        public ComplianceSchemeController(IFeesService feesService)
        {
            _feesService = feesService ?? throw new ArgumentNullException(nameof(feesService));
        }

        [HttpPost("calculate-fees")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RegistrationFeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Calculate fees for compliance scheme registration", Description = "Calculates the registration fees for a compliance scheme based on the number of large producers, small producers, online marketplaces, and subsidiaries.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully calculated fees", typeof(RegistrationFeeResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No fee found for the provided parameters.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<ActionResult<RegistrationFeeResponseDto>> CalculateFeesAsync(
            [FromBody, SwaggerParameter(Description = "Details of the compliance scheme registration request", Required = true)] ComplianceSchemeRegistrationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that the request has at least one producer or subsidiary specified
            if (request.NumberOfLargeProducers == null && request.NumberOfSmallProducers == null &&
                request.NumberOfOnlineMarketplaces == null && request.NumberOfSubsidiaries == null && !request.PayBaseFeeAlone)
            {
                return BadRequest("At least one producer type or subsidiary count must be specified.");
            }

            // Validate the number of subsidiaries if provided
            if (request.NumberOfSubsidiaries != null && (request.NumberOfSubsidiaries < 0 || request.NumberOfSubsidiaries > 100))
            {
                return BadRequest("Number of subsidiaries must be between 0 and 100.");
            }

            try
            {
                var response = await _feesService.CalculateComplianceSchemeFeesAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
