using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
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
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.ComplianceSchemeServiceUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.ComplianceSchemeServiceEndPointNameMissing))
        {
        }

        public async Task<ComplianceSchemeFeesResponseDto> CalculateFeesAsync(ComplianceSchemeFeesRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Post<ComplianceSchemeFeesResponseDto>(UrlConstants.CalculateComplianceSchemeFee, request, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorCalculatingCompianceSchemeFees, ex);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.UnexpectedErrorCalculatingCompianceSchemeFees, ex);
            }
        }
    }
}