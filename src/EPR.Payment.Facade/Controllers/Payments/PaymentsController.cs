using Asp.Versioning;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Services;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.Payments
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentsService _paymentsService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentsService paymentsService, ILogger<PaymentsController> logger)
        {
            _paymentsService = paymentsService ?? throw new ArgumentNullException(nameof(paymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(paymentsService));
        }
   
        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Initiates a new payment", Description = "Initiates a new payment with mandatory payment request data. <br>" +
            "Return_url input paramater is the url that Gov Pay will return back to when the payment journey is complete. <br>" +
            "The reutnurl parameter in the response object is the initial page in the Gov Pay journeny.")]
        [SwaggerResponse(StatusCodes.Status201Created, "Returns the created payment response.", typeof(PaymentResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.")]
        public async Task<ActionResult<PaymentResponseDto>> InitiatePayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _paymentsService.InitiatePaymentAsync(request);
                return CreatedAtAction(nameof(GetPaymentStatus), new { paymentId = result.PaymentId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing InitiatePayment request");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("{paymentId}/status")]
        [ProducesResponseType(typeof(PaymentStatusResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Retrieves the status of a payment", Description = "Retrieves the status of a payment for the paymentId requested.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the payment status response.", typeof(PaymentStatusResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "If the payment is not found.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.")]
        public async Task<ActionResult<PaymentStatusResponseDto>> GetPaymentStatus(string paymentId)
        {
            if (string.IsNullOrEmpty(paymentId))
            {
                return BadRequest("PaymentId cannot be null or empty");
            }

            try
            {
                var paymentStatusResponseDto = await _paymentsService.GetPaymentStatusAsync(paymentId);
                if (paymentStatusResponseDto == null)
                    return NotFound();

                return Ok(paymentStatusResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing GetPaymentStatus request");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("{paymentId}/status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Inserts the status of a payment", Description = "Inserts the status of a payment for the paymentId specified.")]
        [SwaggerResponse(StatusCodes.Status200OK, "If the status is successfully inserted.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.")]
        public async Task<IActionResult> InsertPaymentStatus(
            [SwaggerParameter("The ID of the payment.")] string paymentId,
            [FromBody] PaymentStatusInsertRequestDto request)
        {
            // TODO : PS - need exact model to insert payment and then check valid fields etc
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _paymentsService.InsertPaymentStatusAsync(paymentId, request);
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
