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
    [Route("api/v{version:apiVersion}/producers")]
    [FeatureGate("EnableProducerMultipleFeature")]
    public class ProducersMultipleController : ControllerBase
    {
        private readonly IFeesService _feesService;

        public ProducersMultipleController(IFeesService feesService)
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
            [FromBody, SwaggerParameter(Description = "Number of subsidiaries", Required = true)] int numberOfSubsidiaries)
        {
            if (numberOfSubsidiaries < 0 || numberOfSubsidiaries > 100)
            {
                return BadRequest("Number of subsidiaries must be between 0 and 100.");
            }

            try
            {
                var request = new ProducerRegistrationRequestDto
                {
                    IsLargeProducer = true,
                    NumberOfSubsidiaries = numberOfSubsidiaries
                };
                var response = await _feesService.CalculateProducerFeesAsync(request);
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
            [FromBody, SwaggerParameter(Description = "Number of subsidiaries", Required = true)] int numberOfSubsidiaries)
        {
            if (numberOfSubsidiaries < 0 || numberOfSubsidiaries > 100)
            {
                return BadRequest("Number of subsidiaries must be between 0 and 100.");
            }

            try
            {
                var request = new ProducerRegistrationRequestDto
                {
                    IsLargeProducer = false,
                    NumberOfSubsidiaries = numberOfSubsidiaries
                };
                var response = await _feesService.CalculateProducerFeesAsync(request);
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
        [SwaggerOperation(Summary = "Pay base fee for producer", Description = "Pays the base registration fee for a producer.")]
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
                var request = new ProducerRegistrationRequestDto
                {
                    PayBaseFeeAlone = payBaseFeeAlone
                };
                var response = await _feesService.CalculateProducerFeesAsync(request);
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
