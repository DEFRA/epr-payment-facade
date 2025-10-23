using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees
{
    public class HttpProducerFeesV2Service : BaseHttpService, IHttpProducerFeesV2Service
    {
        public HttpProducerFeesV2Service(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<Service> configMonitor)
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("ProducerFeesV2Service").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.RegistrationFeesServiceBaseUrlMissing),
                   configMonitor.Get("ProducerFeesV2Service").EndPointName
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.RegistrationFeesServiceEndPointNameMissing))
        {
            _ = configMonitor.Get("ProducerFeesV2Service");
        }
   
        public async Task<ProducerFeesResponseDto> CalculateProducerFeesAsync(
            ProducerFeesRequestV2Dto request, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.CalculateProducerRegistrationFees;

            try
            {
                var response = await Post<ProducerFeesResponseDto>(url, request, cancellationToken);
                return response;
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
