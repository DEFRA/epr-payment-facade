using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpOnlinePaymentServiceHealthCheckService : BaseHttpService, IHttpPaymentServiceHealthCheckService
    {
        public HttpOnlinePaymentServiceHealthCheckService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<Service> configMonitor)
            : base(
                httpClient,
                httpContextAccessor,
                configMonitor.Get("PaymentServiceHealthCheck")?.Url
                    ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OnlinePaymentServiceBaseUrlMissing),
                configMonitor.Get("PaymentServiceHealthCheck")?.EndPointName
                    ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OnlinePaymentServiceEndPointNameMissing))
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor), ExceptionMessages.BearerTokenNull);
            }

            if (configMonitor.Get("PaymentServiceHealthCheck") == null)
            {
                throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OfflinePaymentServiceBaseUrlMissing);
            }
        }

        public async Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken)
        {
            return await Get<HttpResponseMessage>(string.Empty, cancellationToken, false);
        }
    }
}