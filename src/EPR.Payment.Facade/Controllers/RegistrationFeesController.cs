using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/[controller]")]
[ApiController]
public class RegistrationFeesController : ControllerBase
{
    private readonly IFeesServiceFactory _feesServiceFactory;
    private readonly ILogger<RegistrationFeesController> _logger;

    public RegistrationFeesController(IFeesServiceFactory feesServiceFactory, ILogger<RegistrationFeesController> logger)
    {
        _feesServiceFactory = feesServiceFactory;
        _logger = logger;
    }

    [HttpGet("small-and-large-producers")]
    [SwaggerOperation(Summary = "Get fees for small and large producers", Description = "Retrieves the fee details for small and large producers.")]
    [SwaggerResponse(200, "Successfully retrieved the fee details.", typeof(GetFeesResponseDto))]
    [SwaggerResponse(404, "No fees found for the specified subtype.")]
    [SwaggerResponse(500, "An error occurred while processing the request.")]
    public async Task<IActionResult> GetSmallAndLargeProducersFees()
    {
        return await GetRegistrationFees(RegistrationFeeSubType.SmallAndLargeProducers);
    }

    [HttpGet("subsidaries")]
    [SwaggerOperation(Summary = "Get fees for subsidaries", Description = "Retrieves the fee details for subsidaries.")]
    [SwaggerResponse(200, "Successfully retrieved the fee details.", typeof(GetFeesResponseDto))]
    [SwaggerResponse(404, "No fees found for the specified subtype.")]
    [SwaggerResponse(500, "An error occurred while processing the request.")]
    public async Task<IActionResult> GetSubsidariesFees()
    {
        return await GetRegistrationFees(RegistrationFeeSubType.Subsidaries);
    }

    [HttpGet("resubmission-and-additional-fees")]
    [SwaggerOperation(Summary = "Get resubmission and additional fees", Description = "Retrieves the fee details for resubmission and additional fees.")]
    [SwaggerResponse(200, "Successfully retrieved the fee details.", typeof(GetFeesResponseDto))]
    [SwaggerResponse(404, "No fees found for the specified subtype.")]
    [SwaggerResponse(500, "An error occurred while processing the request.")]
    public async Task<IActionResult> GetResubmissionAndAdditionalFees()
    {
        return await GetRegistrationFees(RegistrationFeeSubType.ResubmissionAndAdditionalFees);
    }

    [HttpGet("compliance-schemes")]
    [SwaggerOperation(Summary = "Get fees for compliance schemes", Description = "Retrieves the fee details for compliance schemes.")]
    [SwaggerResponse(200, "Successfully retrieved the fee details.", typeof(GetFeesResponseDto))]
    [SwaggerResponse(404, "No fees found for the specified subtype.")]
    [SwaggerResponse(500, "An error occurred while processing the request.")]
    public async Task<IActionResult> GetComplianceSchemesFees()
    {
        return await GetRegistrationFees(RegistrationFeeSubType.ComplianceSchemes);
    }

    [HttpGet("compliance-scheme-plus")]
    [SwaggerOperation(Summary = "Get fees for compliance scheme plus", Description = "Retrieves the fee details for compliance scheme plus.")]
    [SwaggerResponse(200, "Successfully retrieved the fee details.", typeof(GetFeesResponseDto))]
    [SwaggerResponse(404, "No fees found for the specified subtype.")]
    [SwaggerResponse(500, "An error occurred while processing the request.")]
    public async Task<IActionResult> GetComplianceSchemePlusFees()
    {
        return await GetRegistrationFees(RegistrationFeeSubType.ComplianceSchemePlus);
    }

    [HttpGet("compliance-scheme-resubmission-additional-fees")]
    [SwaggerOperation(Summary = "Get fees for compliance scheme resubmission and additional fees", Description = "Retrieves the fee details for compliance scheme resubmission and additional fees.")]
    [SwaggerResponse(200, "Successfully retrieved the fee details.", typeof(GetFeesResponseDto))]
    [SwaggerResponse(404, "No fees found for the specified subtype.")]
    [SwaggerResponse(500, "An error occurred while processing the request.")]
    public async Task<IActionResult> GetComplianceSchemeResubmissionAndAdditionalFees()
    {
        return await GetRegistrationFees(RegistrationFeeSubType.ComplianceSchemeResubmissionAndAdditionalFees);
    }

    private async Task<IActionResult> GetRegistrationFees(RegistrationFeeSubType subType)
    {
        try
        {
            var feesService = _feesServiceFactory.CreateFeesService(subType);
            var feeResponse = await feesService.GetFeesAsync();
            if (feeResponse == null)
            {
                _logger.LogWarning("No registration fees found for subtype: {SubType}", subType);
                return NotFound($"No registration fees found for subtype: {subType}");
            }
            return Ok(feeResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for subtype: {SubType}", subType);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
