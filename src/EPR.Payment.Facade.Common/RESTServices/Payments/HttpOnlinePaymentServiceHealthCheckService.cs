using EPR.Payment.Facade.Common.Configuration;
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
            : base(httpClient,
                    httpContextAccessor,
                    configMonitor?.Get("PaymentServiceHealthCheck")?.Url
                        ?? throw new ArgumentNullException("PaymentServiceHealthCheck BaseUrl configuration is missing"),
                    configMonitor?.Get("PaymentServiceHealthCheck")?.EndPointName
                        ?? throw new ArgumentNullException("PaymentServiceHealthCheck EndPointName configuration is missing"))
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor), "Value cannot be null. (Parameter 'httpContextAccessor')");
            }

            if (configMonitor == null || configMonitor.Get("PaymentServiceHealthCheck") == null)
            {
                throw new ArgumentNullException(nameof(configMonitor), "Value cannot be null or missing required configuration. (Parameter 'configMonitor')");
            }
        }

        public async Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken)
        {
            return await Get<HttpResponseMessage>(string.Empty, cancellationToken, false);
        }
    }
}