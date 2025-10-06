using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme
{
    public class HttpComplianceSchemeFeesServiceV2 : BaseHttpService, IHttpComplianceSchemeFeesServiceV2
    {
        public HttpComplianceSchemeFeesServiceV2(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<Service> configMonitor)
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("ComplianceSchemeFeesServiceV2").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.ComplianceSchemeServiceUrlMissing),
                   configMonitor.Get("ComplianceSchemeFeesServiceV2").EndPointName
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.ComplianceSchemeServiceEndPointNameMissing))
        {
            _ = configMonitor.Get("ComplianceSchemeFeesServiceV2");
        }

        public async Task<ComplianceSchemeFeesResponseDto> CalculateFeesAsync(
            ComplianceSchemeFeesRequestV2Dto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Post<ComplianceSchemeFeesResponseDto>(
                    UrlConstants.CalculateComplianceSchemeFee,
                    request,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorCalculatingComplianceSchemeFees, ex);
            }
            catch (ResponseCodeException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ValidationException(ex.Message.Trim('"'));
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.UnexpectedErrorCalculatingComplianceSchemeFees, ex);
            }
        }
    }
}
