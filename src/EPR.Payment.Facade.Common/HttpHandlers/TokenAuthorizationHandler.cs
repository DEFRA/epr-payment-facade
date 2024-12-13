using Azure.Core;
using Azure.Identity;
using EPR.Payment.Facade.Common.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace EPR.Payment.Facade.Common.HttpHandlers
{
    public class TokenAuthorizationHandler : DelegatingHandler
    {
        private readonly TokenRequestContext _tokenRequestContext;
        private readonly DefaultAzureCredential? _credentials;
        private readonly ILogger<TokenAuthorizationHandler> _logger;

        public TokenAuthorizationHandler(IOptions<Service> serviceConfig, ILogger<TokenAuthorizationHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrEmpty(serviceConfig.Value.ServiceClientId))
            {
                _logger.LogWarning("ServiceClientId is missing in the service configuration.");
                return;
            }

            _tokenRequestContext = new TokenRequestContext(new[] { serviceConfig.Value.ServiceClientId });
            _credentials = new DefaultAzureCredential();

            _logger.LogInformation("TokenAuthorizationHandler initialized with ServiceClientId: {ServiceClientId}.", serviceConfig.Value.ServiceClientId);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_credentials != null)
            {
                try
                {
                    _logger.LogInformation("Attempting to retrieve token for {Scopes}.", _tokenRequestContext.Scopes);

                    var tokenResult = await _credentials.GetTokenAsync(_tokenRequestContext, cancellationToken);

                    _logger.LogInformation("Token retrieved successfully for {Scopes}.", _tokenRequestContext.Scopes);

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

                    _logger.LogInformation("Authorization header added to the request.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve token for {Scopes}.", _tokenRequestContext.Scopes);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("DefaultAzureCredential is not initialized.");
            }

            // Log details about the outgoing request
            _logger.LogInformation("Sending HTTP request to {RequestUri} with method {HttpMethod}.", request.RequestUri, request.Method);

            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                // Log details about the response
                _logger.LogInformation("Received HTTP response with status code {StatusCode} from {RequestUri}.", response.StatusCode, request.RequestUri);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the HTTP request to {RequestUri}.", request.RequestUri);
                throw;
            }
        }
    }
}
