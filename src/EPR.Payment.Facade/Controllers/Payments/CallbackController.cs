using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Internal.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/callback")]
[FeatureGate("EnableCallbackFeature")]
public class CallbackController : ControllerBase
{
    private readonly ICookieService _cookieService;
    private readonly ILogger<CallbackController> _logger;

    public CallbackController(ICookieService cookieService, ILogger<CallbackController> logger)
    {
        _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("retrieve-payment-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Retrieves payment data from the cookie",
        Description = "Retrieves and decrypts the payment data stored in the cookie. The decrypted data is returned as a PaymentCookieDataDto."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved and decrypted payment data.", typeof(PaymentCookieDataDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If the payment data cookie is not found or invalid.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.")]
    [FeatureGate("EnableRetrievePaymentData")]
    public IActionResult RetrievePaymentData()
    {
        try
        {
            var encryptedPaymentData = Request.Cookies["PaymentData"];
            if (string.IsNullOrEmpty(encryptedPaymentData))
            {
                _logger.LogWarning(LogMessages.PaymentDataNotFound);
                return BadRequest(new ProblemDetails
                {
                    Title = "Payment Data Not Found",
                    Detail = "The payment data cookie was not found.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var decryptedPaymentData = _cookieService.RetrievePaymentDataCookie(encryptedPaymentData);
            var paymentData = JsonConvert.DeserializeObject<PaymentCookieDataDto>(decryptedPaymentData);

            return Ok(paymentData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.ErrorRetrievingPaymentData);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving the payment data.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
