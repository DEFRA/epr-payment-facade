using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees
{
    public class HttpRegistrationFeesService : BaseHttpService, IHttpRegistrationFeesService
    {
        private readonly IHttpClientFactory _httpClientFactory = null!;
        private readonly string _httpClientName = null!;

        public HttpRegistrationFeesService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceBaseUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceEndPointNameMissing))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClientName = config.Value.HttpClientName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceHttpClientNameMissing);
        }

        public async Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationFeeRequestDto request, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.CalculateProducerRegistrationFees;
            try
            {
                var response = await Post<RegistrationFeeResponseDto>(url, request, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorCalculatingProducerFees, ex);
            }
        }
    }
}
