using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme
{
    public class HttpComplianceSchemeResubmissionFeesService : BaseHttpService, IHttpComplianceSchemeResubmissionFeesService
    {
        public HttpComplianceSchemeResubmissionFeesService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceBaseUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceEndPointNameMissing))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<ComplianceSchemeResubmissionFeeResult> CalculateResubmissionFeeAsync(ComplianceSchemeResubmissionFeeRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Post<ComplianceSchemeResubmissionFeeResult>(UrlConstants.GetComplianceSchemeResubmissionFee, request, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorResubmissionFees, ex);
            }
        }
    }
}