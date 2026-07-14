using System.Net;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission
{
    public class HttpRegistrationSubmissionDataService : BaseHttpService, IHttpRegistrationSubmissionDataService
    {
        public HttpRegistrationSubmissionDataService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<Service> configMonitor)
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("RegistrationSubmissionDataService").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.RegistrationSubmissionDataServiceBaseUrlMissing),
                   configMonitor.Get("RegistrationSubmissionDataService").EndPointName
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.RegistrationFeesServiceEndPointNameMissing))
        {
        }

        public async Task<IReadOnlyList<RegistrationFeeCalculationDetailsDto>?> GetFeeCalculationDetailsAsync(Guid submissionId, CancellationToken cancellationToken = default)
        {
            var url = $"{UrlConstants.RegistrationSubmissionData}/{submissionId}/fee-calculation-details";

            try
            {
                return await Get<List<RegistrationFeeCalculationDetailsDto>>(url, cancellationToken, includeTrailingSlash: false);
            }
            catch (ResponseCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorRetrievingRegistrationFeeCalculationDetails, ex);
            }
        }
    }
}
