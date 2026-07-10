using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees
{
    public class HttpSubmissionPeriodsService : BaseHttpService, IHttpSubmissionPeriodsService
    {
        public HttpSubmissionPeriodsService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<Service> configMonitor)
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("SubmissionPeriodsService").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.SubmissionPeriodsServiceBaseUrlMissing),
                   configMonitor.Get("SubmissionPeriodsService").EndPointName
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.SubmissionPeriodsServiceEndPointNameMissing))
        {
        }

        public async Task<IReadOnlyList<SubmissionPeriodResponseDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await Get<List<SubmissionPeriodResponseDto>>(UrlConstants.SubmissionPeriods, cancellationToken, includeTrailingSlash: false);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorRetrievingSubmissionPeriods, ex);
            }
        }
    }
}
