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
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("PaymentServiceHealthCheck").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), "PaymentServiceHealthCheck BaseUrl configuration is missing"),
                   configMonitor.Get("PaymentServiceHealthCheck").EndPointName
                       ?? throw new ArgumentNullException(nameof(configMonitor), "PaymentServiceHealthCheck EndPointName configuration is missing"))
        {
            var config = configMonitor.Get("PaymentServiceHealthCheck");
            Console.WriteLine($"HttpOnlinePaymentServiceHealthCheckService initialized with BaseUrl: {config.Url}");
        }

        public async Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken)
        {
            return await Get<HttpResponseMessage>(string.Empty, cancellationToken, false);
        }
    }
}
