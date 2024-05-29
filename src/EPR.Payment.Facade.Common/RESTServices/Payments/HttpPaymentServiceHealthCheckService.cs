using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpPaymentServiceHealthCheckService : BaseHttpService, IHttpPaymentServiceHealthCheckService
    {
        public HttpPaymentServiceHealthCheckService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            string baseUrl,
            string endPointName) : base(httpContextAccessor, httpClientFactory, baseUrl, endPointName)
        {
        }

        public async Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken)
        {
            return await Get<HttpResponseMessage>(string.Empty, false);
        }
    }
}