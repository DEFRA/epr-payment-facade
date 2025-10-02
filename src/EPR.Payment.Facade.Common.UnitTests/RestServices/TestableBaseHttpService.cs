using EPR.Payment.Facade.Common.RESTServices;
using Microsoft.AspNetCore.Http;

namespace EPR.Payment.Facade.Common.UnitTests.RESTServices
{
    public class TestableBaseHttpService : BaseHttpService
    {
        public TestableBaseHttpService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        string baseUrl,
        string endPointName)
        : base(httpClient, httpContextAccessor, baseUrl, endPointName)
        {
        }

        // Expose the protected _baseUrl for testing purposes
        public string BaseUrl => _baseUrl;

        // Public wrappers for protected methods for testing purposes

        public Task<T> PublicGet<T>(string url, CancellationToken cancellationToken) =>
            Get<T>(url, cancellationToken);

        public Task<T> PublicPost<T>(string url, object payload, CancellationToken cancellationToken) =>
            Post<T>(url, payload, cancellationToken);

        public Task PublicPost(string url, object payload, CancellationToken cancellationToken) =>
            Post(url, payload, cancellationToken);

        public Task<T> PublicPut<T>(string url, object payload, CancellationToken cancellationToken) =>
            Put<T>(url, payload, cancellationToken);

        public Task PublicPut(string url, object payload, CancellationToken cancellationToken) =>
            Put(url, payload, cancellationToken);

        public Task<T> PublicDelete<T>(string url, object payload, CancellationToken cancellationToken) =>
            Delete<T>(url, payload, cancellationToken);

        public Task PublicDelete(string url, object payload, CancellationToken cancellationToken) =>
            Delete(url, payload, cancellationToken);

        public void PublicSetBearerToken(string token) =>
            SetBearerToken(token);
    }
}
