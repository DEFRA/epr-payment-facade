using System.ComponentModel.DataAnnotations;
using System.Net;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.AccreditationFees.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.AccreditationFees
{
    public class HttpAccreditationFeesCalculatorService : BaseHttpService, IHttpAccreditationFeesCalculatorService
    {
        public HttpAccreditationFeesCalculatorService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<Service> configMonitor)
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("ReprocessorOrExporterAccreditationFeesService").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.RegistrationFeesServiceBaseUrlMissing),
                   configMonitor.Get("ReprocessorOrExporterAccreditationFeesService").EndPointName
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.RegistrationFeesServiceEndPointNameMissing))
        {
        }

        public async Task<ReprocessorOrExporterAccreditationFeesResponseDto?> CalculateAccreditationFeesAsync(
            ReprocessorOrExporterAccreditationFeesRequestDto accreditationFeesRequestDto,
            CancellationToken cancellationToken)
        {
            var url = UrlConstants.CalculateReprocessorOrExporterAccreditationFees;

            try
            {
                ReprocessorOrExporterAccreditationFeesResponseDto accreditationFeesResponseDto = await Post<ReprocessorOrExporterAccreditationFeesResponseDto>(url, accreditationFeesRequestDto, cancellationToken);
                
                return accreditationFeesResponseDto;
            }
            catch (ResponseCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            catch (ResponseCodeException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ValidationException(ex.Message.Trim('"'));
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorCalculatingProducerFees, ex);
            }
        }
    }
}
