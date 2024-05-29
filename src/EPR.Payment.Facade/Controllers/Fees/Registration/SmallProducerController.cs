using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/v{version:apiVersion}/registration/small-producer")]
[ApiVersion(1)]
public class SmallProducerController : ControllerBase
{
    private readonly IFeeCalculatorFactory _feeCalculatorFactory;
    private readonly ILogger<SmallProducerController> _logger;

    public SmallProducerController(IFeeCalculatorFactory feeCalculatorFactory, ILogger<SmallProducerController> logger)
    {
        _feeCalculatorFactory = feeCalculatorFactory;
        _logger = logger;
    }

    [HttpGet("calculate-fee")]
    [MapToApiVersion(1)]
    [SwaggerOperation(
        Summary = "Calculate registration fee for a small producer.",
        Description = "Calculate registration fee for a small producer based on provided parameters."
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
            _logger.LogError(ex, "An error occurred while calculating registration fee for a small producer.");
            return StatusCode(500, new ProblemDetails { Title = "Internal Server Error", Detail = "An error occurred while processing the request." });
        }
    }
}
