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

        /// <summary>
        /// Retrieves fee information based on the provided parameters.
        /// </summary>
        /// <param name="isLarge">Specifies if the fee is for a large transaction.</param>
        /// <param name="regulator">The regulator for which the fee is being retrieved.</param>
        /// <returns>
        /// An ActionResult of type GetFeesResponseDto representing the fee information,
        /// or a 404 Not Found if no fee information is found for the specified parameters.
        /// </returns>
        [MapToApiVersion(1)]
        [HttpGet]
        [ProducesResponseType(typeof(GetFeesResponseDto), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetFeesResponseDto>> GetFee(bool isLarge, string regulator)
        {
            // Check for invalid parameters
            if (string.IsNullOrEmpty(regulator))
            {
                return BadRequest("Invalid 'regulator' parameter provided");
            }

            try
            {
                var feeResponse = await _feesService.GetFee(isLarge, regulator);
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
