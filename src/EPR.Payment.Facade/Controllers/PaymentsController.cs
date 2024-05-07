using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Services;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Payment.Facade.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentsService _paymentsService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentsService paymentsService, ILogger<PaymentsController> logger)
        {
            _paymentsService = paymentsService ?? throw new ArgumentNullException(nameof(paymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(paymentsService));
        }       

        [MapToApiVersion(1)]
        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponseDto), 201)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentResponseDto>> InitiatePayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _paymentsService.InitiatePayment(request);
                return CreatedAtAction(nameof(GetPaymentStatus), new { paymentId = result.PaymentId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing InitiatePayment request");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [MapToApiVersion(1)]
        [HttpGet("{paymentId}/status")]
        [ProducesResponseType(typeof(PaymentStatusResponseDto), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentStatusResponseDto>> GetPaymentStatus(string paymentId)
        {            
            if (string.IsNullOrEmpty(paymentId))
            {
                return BadRequest("PaymentId cannot be null or empty");
            }

            try
            {
                var paymentStatusResponseDto = await _paymentsService.GetPaymentStatus(paymentId);
                if (paymentStatusResponseDto == null)
                    return NotFound();

                return Ok(paymentStatusResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing GetPaymentStatus request");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }


        [MapToApiVersion(1)]
        [HttpPost("{paymentId}/status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertPaymentStatus(string paymentId, [FromBody] PaymentStatusInsertRequestDto request)
        {
            // TODO : PS - need exact model to insert payment and then check valid fields etc
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _paymentsService.InsertPaymentStatus(paymentId, request);
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
