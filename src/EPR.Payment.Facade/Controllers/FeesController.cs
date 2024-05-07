using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetFeesResponseDto>> GetFee(bool isLarge, string regulator)
        {
            try
            {
                var feeResponse = await _feesService.GetFee(isLarge, regulator);
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
