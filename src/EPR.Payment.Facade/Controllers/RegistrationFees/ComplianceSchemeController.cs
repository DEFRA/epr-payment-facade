using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Controllers.RegistrationFees
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/compliancescheme")]
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

            var validationError = ValidateComplianceSchemeRequest(request);
            if (validationError != null)
            {
                return BadRequest(validationError);
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
                // Log the exception if necessary
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private string ValidateComplianceSchemeRequest(ComplianceSchemeRegistrationRequestDto request)
        {
            foreach (var producer in request.Producers)
            {
                if (producer.NumberOfSubsidiaries < 0 || producer.NumberOfSubsidiaries > 100)
                {
                    return "Number of subsidiaries per producer must be between 0 and 100.";
                }

                if (producer.ProducerType != "L" && producer.ProducerType != "S")
                {
                    return "ProducerType must be 'L' for Large or 'S' for Small.";
                }
            }

            if (!request.PayComplianceSchemeBaseFee && !request.Producers.Any(p => p.PayBaseFee) && !request.Producers.Any(p => p.NumberOfSubsidiaries > 0))
            {
                return "No valid fees requested.";
            }

            return null;
        }
    }
}
