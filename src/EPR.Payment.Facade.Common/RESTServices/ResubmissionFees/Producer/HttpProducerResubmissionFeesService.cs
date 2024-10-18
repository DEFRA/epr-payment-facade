using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer
{
    public class HttpProducerResubmissionFeesService : BaseHttpService, IHttpProducerResubmissionFeesService
    {
        public HttpProducerResubmissionFeesService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceBaseUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceEndPointNameMissing))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<decimal?> GetResubmissionFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.GetProducerResubmissionFee.Replace("{regulator}", request.Regulator);
            try
            {
                var response = await Get<decimal>(url, cancellationToken, false);
                return response;
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorResubmissionFees, ex);
            }
        }
    }
}