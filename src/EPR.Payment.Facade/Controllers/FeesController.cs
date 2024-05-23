using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/[controller]")]
    public class FeesController : ControllerBase
    {
        private readonly IFeesService _feesService;
        private readonly ILogger<FeesController> _logger;

        public FeesController(IFeesService feesService, ILogger<FeesController> logger)
        {
            _feesService = feesService ?? throw new ArgumentNullException(nameof(feesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [MapToApiVersion(1)]
        [HttpGet]
        [ProducesResponseType(typeof(GetFeesResponseDto), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Get fees", Description = "Retrieves fee information based on the provided parameters.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the requested fees.", typeof(GetFeesResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid 'regulator' parameter provided.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No fee found for the provided parameters.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
        public async Task<ActionResult<GetFeesResponseDto>> GetFee(
            [SwaggerParameter("Determines whether to retrieve large fees.")]  bool isLarge,
            [SwaggerParameter("The regulator for which fee information is requested.")] string regulator)
        {
            // Check for invalid parameters
            if (string.IsNullOrEmpty(regulator))
            {
                return BadRequest("Invalid 'regulator' parameter provided");
            }

            try
            {
                var feeResponse = await _feesService.GetFeeAsync(isLarge, regulator);
                if (feeResponse == null)
                {
                    return NotFound();
                }

                return Ok(feeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing GetFee request");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

    }
}
