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
            : base(
                httpClient,
                httpContextAccessor,
                configMonitor?.Get("ProducerFeesService")?.Url
                    ?? throw new ArgumentNullException(nameof(configMonitor), "ProducerFeesService BaseUrl configuration is missing"),
                configMonitor?.Get("ProducerFeesService")?.EndPointName
                    ?? throw new ArgumentNullException(nameof(configMonitor), "ProducerFeesService EndPointName configuration is missing"))
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor), "Value cannot be null. (Parameter 'httpContextAccessor')");
            }

            if (configMonitor == null || configMonitor.Get("ProducerFeesService") == null)
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