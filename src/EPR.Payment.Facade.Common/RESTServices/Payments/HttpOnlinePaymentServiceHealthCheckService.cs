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
            IOptions<Service> config)
            : base(httpClient,
                   httpContextAccessor,
                   config.Value.Url ?? throw new ArgumentNullException(nameof(config), "PaymentServiceHealthCheck BaseUrl configuration is missing"))
        {
        }

        public async Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken)
        {
            return await Get<HttpResponseMessage>(string.Empty, cancellationToken, false);
        }
    }
}
