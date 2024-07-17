using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
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
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceBaseUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceEndPointNameMissing))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _bearerToken = config.Value.BearerToken ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.BearerTokenNull);
        }

        public async Task<GovPayResponseDto> InitiatePaymentAsync(GovPayRequestDto paymentRequestDto, CancellationToken cancellationToken)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {
                throw new InvalidOperationException(ExceptionMessages.BearerTokenNull);
            }

            var url = UrlConstants.GovPayInitiatePayment;
            try
            {
                return await Post<GovPayResponseDto>(url, paymentRequestDto, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionMessages.ErrorInitiatingPayment, ex);
            }
        }

        public async Task<PaymentStatusResponseDto?> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {
                throw new InvalidOperationException(ExceptionMessages.BearerTokenNull);
            }

            var url = UrlConstants.GovPayGetPaymentStatus.Replace("{paymentId}", paymentId);
            try
            {
                return await Get<PaymentStatusResponseDto>(url, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionMessages.ErrorRetrievingPaymentStatus, ex);
            }
        }
    }
}
