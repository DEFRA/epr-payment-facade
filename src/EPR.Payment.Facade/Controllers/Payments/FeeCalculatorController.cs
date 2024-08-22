using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using Microsoft.AspNetCore.Mvc;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFee;
using EPR.Payment.Facade.Examples;
using System.Text.Json;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFee;

namespace EPR.Payment.Facade.Controllers.Payments;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class FeeCalculatorController(ILogger<PaymentsController> logger) : ControllerBase
{
    private readonly ILogger<PaymentsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    /// <summary>
    /// Producer registration fee calculator.
    /// </summary>
    /// <remarks>
    /// This end point will be used when a producer needs to pay for their registration and there is no late fee or resubmission fee to be applied.
    /// </remarks>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Return total registration fee along with fee breakdown for Subsidiary.</response>
    /// <response code="400">If the request is invalid or a validation error occurs.</response>
    /// <returns>This end point will be used when a producer needs to pay for their registration and there is no late fee or resubmission fee to be applied.</returns>
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(CalculateRegistrationFeeResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(CalculateRegistrationFeeRequestDto), typeof(CalculateRegistrationFeeRequestExample))]
    [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(CalculateRegistrationFeeResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ProblemDetailsExample))]
    public Task<IActionResult> CalculateRegistrationFee([FromBody] CalculateRegistrationFeeRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fee cal API called with request {RequestString}", JsonSerializer.Serialize(request));
        return !this.ModelState.IsValid ?
                   Task.FromResult<IActionResult>(this.BadRequest(this.ModelState))
                   : Task.FromResult<IActionResult>(this.Ok());
    }
}