using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Payment.Facade.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/[controller]")]    
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(paymentService));
        }

        [MapToApiVersion(1)]
        [HttpGet("fee")]
        [ProducesResponseType(typeof(GetFeeResponseDto), 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetFeeResponseDto>> GetFee(bool isLarge, string regulator)
        {
            try
            {
                var feeResponse = await _paymentService.GetFee(isLarge, regulator);
                return Ok(feeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing GetFee request");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [MapToApiVersion(1)]
        [HttpPost("initiate")]
        [ProducesResponseType(typeof(PaymentResponseDto), 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentResponseDto>> InitiatePayment([FromBody] PaymentRequestDto request)
        {
            try
            {
                var result = await _paymentService.InitiatePayment(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing InitiatePayment request");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [MapToApiVersion(1)]
        [HttpGet("status/{paymentId}")]
        [ProducesResponseType(typeof(PaymentStatusResponseDto), 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentStatusResponseDto>> GetPaymentStatus(string paymentId)
        {
            try
            {
                var paymentStatusResponseDto = await _paymentService.GetPaymentStatus(paymentId);
                return Ok(paymentStatusResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing GetPaymentStatus request");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("status/{paymentId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertPaymentStatus(string paymentId, [FromBody] PaymentStatusInsertRequestDto request)
        {
            try
            {                
                await _paymentService.InsertPaymentStatus(paymentId, request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing InsertPaymentStatus request");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }

}
