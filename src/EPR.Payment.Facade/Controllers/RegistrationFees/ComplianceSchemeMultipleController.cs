using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/complianceschemes")]
    [FeatureGate("EnableComplianceSchemeMultipleFeature")]
    public class ComplianceSchemeMultipleController : ControllerBase
    {
        private readonly IFeesService _feesService;

        public ComplianceSchemeMultipleController(IFeesService feesService)
        {
            _feesService = feesService ?? throw new ArgumentNullException(nameof(feesService));
        }

        [HttpPost("calculate-large-producer-fees")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RegistrationFeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Calculate fees for large producer registration", Description = "Calculates the registration fees for large producers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully calculated fees", typeof(RegistrationFeeResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No fee found for the provided parameters.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<ActionResult<RegistrationFeeResponseDto>> CalculateLargeProducerFeesAsync(
            [FromBody, SwaggerParameter(Description = "Number of large producers", Required = true)] int numberOfLargeProducers)
        {
            if (numberOfLargeProducers <= 0)
            {
                return BadRequest("Number of large producers must be greater than 0.");
            }

            try
            {
                var request = new ComplianceSchemeRegistrationRequestDto
                {
                    NumberOfLargeProducers = numberOfLargeProducers
                };
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

        [HttpPost("calculate-small-producer-fees")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RegistrationFeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Calculate fees for small producer registration", Description = "Calculates the registration fees for small producers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully calculated fees", typeof(RegistrationFeeResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No fee found for the provided parameters.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<ActionResult<RegistrationFeeResponseDto>> CalculateSmallProducerFeesAsync(
            [FromBody, SwaggerParameter(Description = "Number of small producers", Required = true)] int numberOfSmallProducers)
        {
            if (numberOfSmallProducers <= 0)
            {
                return BadRequest("Number of small producers must be greater than 0.");
            }

            try
            {
                var request = new ComplianceSchemeRegistrationRequestDto
                {
                    NumberOfSmallProducers = numberOfSmallProducers
                };
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

        [HttpPost("calculate-online-marketplace-fees")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RegistrationFeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Calculate fees for online marketplace registration", Description = "Calculates the registration fees for online marketplace producers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully calculated fees", typeof(RegistrationFeeResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No fee found for the provided parameters.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<ActionResult<RegistrationFeeResponseDto>> CalculateOnlineMarketplaceFeesAsync(
            [FromBody, SwaggerParameter(Description = "Number of online marketplace producers", Required = true)] int numberOfOnlineMarketplaces)
        {
            if (numberOfOnlineMarketplaces <= 0)
            {
                return BadRequest("Number of online marketplaces must be greater than 0.");
            }

            try
            {
                var request = new ComplianceSchemeRegistrationRequestDto
                {
                    NumberOfOnlineMarketplaces = numberOfOnlineMarketplaces
                };
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

        [HttpPost("calculate-subsidiaries-fees")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RegistrationFeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Calculate fees for subsidiaries", Description = "Calculates the registration fees for subsidiaries.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully calculated fees", typeof(RegistrationFeeResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        public async Task<ActionResult<RegistrationFeeResponseDto>> CalculateSubsidiariesFeesAsync(
            [FromBody, SwaggerParameter(Description = "Number of subsidiaries", Required = true)] int numberOfSubsidiaries)
        {
            if (numberOfSubsidiaries <= 0 || numberOfSubsidiaries > 100)
            {
                return BadRequest("Number of subsidiaries must be between 0 and 100.");
            }

            try
            {
                var request = new ComplianceSchemeRegistrationRequestDto
                {
                    NumberOfSubsidiaries = numberOfSubsidiaries
                };
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

        [HttpPost("pay-base-fee")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RegistrationFeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Pay base fee for compliance scheme", Description = "Pays the base registration fee for a compliance scheme.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully paid base fee", typeof(RegistrationFeeResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No fee found for the provided parameters.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<ActionResult<RegistrationFeeResponseDto>> PayBaseFeeAsync(
            [FromBody, SwaggerParameter(Description = "Request to pay base fee", Required = true)] bool payBaseFeeAlone)
        {
            if (!payBaseFeeAlone)
            {
                return BadRequest("Invalid request to pay base fee alone.");
            }

            try
            {
                var request = new ComplianceSchemeRegistrationRequestDto
                {
                    PayBaseFeeAlone = payBaseFeeAlone
                };
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
