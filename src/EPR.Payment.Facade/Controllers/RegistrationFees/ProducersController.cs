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
    [FeatureGate("EnableProducerFeature")]
    public class ProducersController : ControllerBase
    {
        private readonly IFeesService _feesService;

        public ProducersController(IFeesService feesService)
        {
            _feesService = feesService ?? throw new ArgumentNullException(nameof(feesService));
        }

        [HttpPost("calculate-fees")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RegistrationFeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Calculate fees for producer registration", Description = "Calculates the registration fees for a producer based on whether they are a large producer and the number of subsidiaries.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully calculated fees", typeof(RegistrationFeeResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No fee found for the provided parameters.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<ActionResult<RegistrationFeeResponseDto>> CalculateFeesAsync(
            [FromBody, SwaggerParameter(Description = "Details of the producer registration request", Required = true)] ProducerRegistrationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
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
