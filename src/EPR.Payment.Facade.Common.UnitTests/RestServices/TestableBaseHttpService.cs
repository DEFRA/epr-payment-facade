using EPR.Payment.Facade.Common.RESTServices;
using Microsoft.AspNetCore.Http;

namespace EPR.Payment.Facade.Common.UnitTests.RestServices
{
    public class TestableBaseHttpService : BaseHttpService
    {
        public TestableBaseHttpService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            string baseUrl,
            string endPointName)
            : base(httpContextAccessor, httpClientFactory, baseUrl, endPointName)
        {
        }

        // Public wrappers for protected methods for testing purposes
        public Task<T> PublicPost<T>(string url, object payload, CancellationToken cancellationToken) =>
            Post<T>(url, payload, cancellationToken);

        public void PublicSetBearerToken(string token) =>
            SetBearerToken(token);
    }
}
