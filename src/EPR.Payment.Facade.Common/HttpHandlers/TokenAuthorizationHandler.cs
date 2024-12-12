using Azure.Core;
using Azure.Identity;
using EPR.Payment.Facade.Common.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace EPR.Payment.Facade.Common.HttpHandlers
{
    public class TokenAuthorizationHandler : DelegatingHandler
    {
        private readonly TokenRequestContext _tokenRequestContext;
        private readonly DefaultAzureCredential? _credentials;

        public TokenAuthorizationHandler(IOptions<Service> serviceConfig)
        {
            if (string.IsNullOrEmpty(serviceConfig.Value.ServiceClientId))
            {
                return;
            }

            _tokenRequestContext = new TokenRequestContext(new[] { serviceConfig.Value.ServiceClientId });
            _credentials = new DefaultAzureCredential();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_credentials != null)
            {
                var tokenResult = await _credentials.GetTokenAsync(_tokenRequestContext, cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}