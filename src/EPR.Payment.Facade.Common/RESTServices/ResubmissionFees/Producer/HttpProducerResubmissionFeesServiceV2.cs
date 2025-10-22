using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer
{
    public class HttpProducerResubmissionFeesServiceV2 : BaseHttpService, IHttpProducerResubmissionFeesServiceV2
    {
        public HttpProducerResubmissionFeesServiceV2(
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

        public async Task<ProducerResubmissionFeeResponseDto> GetResubmissionFeeAsync(
            ProducerResubmissionFeeRequestV2Dto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Post<ProducerResubmissionFeeResponseDto>(UrlConstants.GetProducerResubmissionFee, request, cancellationToken);
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