using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme
{
    public class HttpComplianceSchemeFeesService : BaseHttpService, IHttpComplianceSchemeFeesService
    {
        public HttpComplianceSchemeFeesService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.ComplianceSchemeServiceBaseUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.ComplianceSchemeServiceEndPointNameMissing))
        {
        }

        public async Task<ComplianceSchemeBaseFeeResponse> GetComplianceSchemeBaseFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee);
            }

            var url = UrlConstants.GetComplianceSchemeBaseFee.Replace("{regulator}", request.Regulator);
            try
            {
                var response = await Get<ComplianceSchemeBaseFeeResponse>(url, cancellationToken, false);

                if (response.BaseFee == 0)
                {
                    throw new ServiceException(string.Format(ExceptionMessages.InvalidRegulatorForComplianceScheme, request.Regulator));
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee, ex);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.UnexpectedErrorRetrievingComplianceSchemeBaseFee, ex);
            }
        }
    }
}