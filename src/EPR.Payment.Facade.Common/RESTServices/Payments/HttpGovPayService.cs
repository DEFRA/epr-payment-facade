using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpGovPayService : BaseHttpService, IHttpGovPayService
    {
        private readonly string? _bearerToken;

        public HttpGovPayService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), "GovPay BaseUrl configuration is missing"),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), "GovPay EndPointName configuration is missing"))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _bearerToken = config.Value.BearerToken ?? throw new ArgumentNullException(nameof(config), "GovPay Bearer token configuration is missing");
        }

        public async Task<PaymentResponseDto> InitiatePaymentAsync(GovPayPaymentRequestDto paymentRequestDto)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {
                throw new InvalidOperationException("Bearer token is null. Unable to initiate payment.");
            }

            var url = "payments";
            try
            {
                return await Post<PaymentResponseDto>(url, paymentRequestDto);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while initiating payment.", ex);
            }
        }

        public async Task<PaymentStatusResponseDto> GetPaymentStatusAsync(string paymentId)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {
                throw new InvalidOperationException("Bearer token is null. Unable to retrieve payment status.");
            }

            var url = $"payments/{paymentId}";
            try
            {
                return await Get<PaymentStatusResponseDto>(url);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving payment status.", ex);
            }
        }
    }
}
