using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Controllers.Fees
{
    [ApiController]
    [Route("api/fees/large-producer")]
    [ApiVersion(1)]
    public class LargeProducerController : ControllerBase
    {
        private readonly IFeeCalculatorFactory _feeCalculatorFactory;
        private readonly ILogger<LargeProducerController> _logger;

        public LargeProducerController(IFeeCalculatorFactory feeCalculatorFactory, ILogger<LargeProducerController> logger)
        {
            _feeCalculatorFactory = feeCalculatorFactory;
            _logger = logger;
        }

        [HttpPost("calculate")]
        [MapToApiVersion(1)]
        [SwaggerOperation(
            Summary = "Calculate large producer fee.",
            Description = "Calculate large producer fee based on provided parameters."
        )]
        [SwaggerResponse(200, "Successful operation", typeof(FeeResponse))]
        [SwaggerResponse(400, "Bad request", typeof(ValidationProblemDetails))]
        [SwaggerResponse(500, "Internal server error", typeof(ProblemDetails))]
        [ProducesResponseType(typeof(FeeResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<ActionResult<FeeResponse>> CalculateFee([FromBody] RegistrationFeeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var feeCalculator = _feeCalculatorFactory.GetFeeCalculator<RegistrationFeeRequest>();
                var response = await feeCalculator.CalculateFeeAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while calculating large producer fee.");
                return StatusCode(500, new ProblemDetails { Title = "Internal Server Error", Detail = "An error occurred while processing the request." });
            }
        }
    }
}
