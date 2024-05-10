using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EPR.Payment.Facade.Common.RESTServices
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

        public async Task<HttpResponseMessage> GetHealth(CancellationToken cancellationToken)
        {
            return await Get<HttpResponseMessage>(string.Empty,false);
        }
    }
}