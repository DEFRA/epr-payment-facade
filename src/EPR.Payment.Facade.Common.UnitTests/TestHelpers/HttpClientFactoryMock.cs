namespace EPR.Payment.Facade.Common.UnitTests.TestHelpers
{
    public class HttpClientFactoryMock : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public HttpClientFactoryMock(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}
