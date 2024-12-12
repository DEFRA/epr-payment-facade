using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme
{
    public class HttpComplianceSchemeResubmissionFeesService : BaseHttpService, IHttpComplianceSchemeResubmissionFeesService
    {
        public HttpComplianceSchemeResubmissionFeesService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<Service> config)
            : base(httpClient,
                   httpContextAccessor,
                   config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceBaseUrlMissing))
        {
        }

        public async Task<ComplianceSchemeResubmissionFeeResponse> CalculateResubmissionFeeAsync(ComplianceSchemeResubmissionFeeRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Post<ComplianceSchemeResubmissionFeeResponse>(UrlConstants.GetComplianceSchemeResubmissionFee, request, cancellationToken);
            }
            catch (ResponseCodeException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ValidationException(ex.Message.Trim('"'));
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorResubmissionFees, ex);
            }
        }
    }
}
