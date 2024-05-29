using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[ApiController]
[Route("api/v{version:apiVersion}/registration/large-producer")]
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

    [HttpGet("calculate-fee")]
    [MapToApiVersion(1)]
    [SwaggerOperation(
        Summary = "Calculate registration fee for a large producer.",
        Description = "Calculate registration fee for a large producer based on provided parameters."
    )]
    [SwaggerResponse(200, "Successful operation", typeof(FeeResponse))]
    [SwaggerResponse(400, "Bad request", typeof(ValidationProblemDetails))]
    [SwaggerResponse(500, "Internal server error", typeof(ProblemDetails))]
    [ProducesResponseType(typeof(FeeResponse), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<ActionResult<FeeResponse>> CalculateFee([FromQuery]
        [SwaggerParameter("Fee calculation request object")]
        RegistrationFeeRequest request)
    {
        try
        {
            // Validate input
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
            _logger.LogError(ex, "An error occurred while calculating registration fee for a large producer.");
            return StatusCode(500, new ProblemDetails { Title = "Internal Server Error", Detail = "An error occurred while processing the request." });
        }
    }
}
