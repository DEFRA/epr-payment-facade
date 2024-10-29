using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpOnlinePaymentServiceHealthCheckService : BaseHttpService, IHttpPaymentServiceHealthCheckService
    {
        public HttpOnlinePaymentServiceHealthCheckService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), "PaymentServiceHealthCheck BaseUrl configuration is missing"),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), "PaymentServiceHealthCheck EndPointName configuration is missing"))
        {
        }

        public async Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken)
        {
            return await Get<HttpResponseMessage>(string.Empty, cancellationToken,false);
        }

    }
}
