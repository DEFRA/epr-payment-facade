using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ReprocessorOrExporter.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ReprocessorOrExporter
{
    public class HttpReprocessorExporterRegistrationFeesService : BaseHttpService, IHttpReprocessorExporterRegistrationFeesService
    {
        public HttpReprocessorExporterRegistrationFeesService(
           HttpClient httpClient,
           IHttpContextAccessor httpContextAccessor,
           IOptionsMonitor<Service> configMonitor)
           : base(httpClient,
                  httpContextAccessor,
                  configMonitor.Get("RexExpoRegistrationFeesService").Url
                      ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.ReproExpoRegServiceUrlMissing),
                  configMonitor.Get("RexExpoRegistrationFeesService").EndPointName
                      ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.ExpoRegServiceEndPointNameMissing))
        {
            var config = configMonitor.Get("RexExpoRegistrationFeesService");
        }
        public async Task<ReprocessorOrExporterRegistrationFeesResponseDto?> CalculateFeesAsync(ReprocessorOrExporterRegistrationFeesRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Post<ReprocessorOrExporterRegistrationFeesResponseDto>(
                    UrlConstants.CalculateRexExpoRegistrationFee,
                    request,
                    cancellationToken);
            }
            catch (ResponseCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.UnexpectedErroreproExpoRegServiceFees, ex);
            }
        }
    }
}