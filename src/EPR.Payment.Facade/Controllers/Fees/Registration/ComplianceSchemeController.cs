using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[ApiController]
[Route("api/v{version:apiVersion}/registration/compliance-scheme")]
[ApiVersion("1.0")]
public class ComplianceSchemeController : ControllerBase
{
    private readonly IFeeCalculatorFactory _feeCalculatorFactory;
    private readonly ILogger<ComplianceSchemeController> _logger;

    public ComplianceSchemeController(IFeeCalculatorFactory feeCalculatorFactory, ILogger<ComplianceSchemeController> logger)
    {
        _feeCalculatorFactory = feeCalculatorFactory;
        _logger = logger;
    }

    [HttpGet("calculate-fee")]
    [SwaggerOperation(
        Summary = "Calculate compliance fee.",
        Description = "Calculate compliance fee based on provided parameters."
    )]
    [SwaggerResponse(200, "Successful operation", typeof(FeeResponse))]
    [SwaggerResponse(400, "Bad request", typeof(ValidationProblemDetails))]
    [SwaggerResponse(500, "Internal server error", typeof(ProblemDetails))]
    [ProducesResponseType(typeof(FeeResponse), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<ActionResult<FeeResponse>> CalculateFee([FromQuery]
        [SwaggerParameter("Compliance fee calculation request object")]
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
            _logger.LogError(ex, "An error occurred while calculating compliance fee.");
            return StatusCode(500, new ProblemDetails { Title = "Internal Server Error", Detail = "An error occurred while processing the request." });
        }
    }
}
