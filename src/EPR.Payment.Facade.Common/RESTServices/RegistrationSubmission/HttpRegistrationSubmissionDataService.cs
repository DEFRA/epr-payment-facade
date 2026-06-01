using System.Net;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission.Interfaces;
using FluentValidation;
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

        public async Task<Guid> CreateAsync(CreateRegistrationSubmissionDataRequest request, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.RegistrationSubmissionData;

            try
            {
                return await Post<Guid>(url, request, cancellationToken);
            }
            catch (ResponseCodeException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ValidationException(ex.Message.Trim('"'));
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorCreatingRegistrationSubmissionData, ex);
            }
        }
    }
}
