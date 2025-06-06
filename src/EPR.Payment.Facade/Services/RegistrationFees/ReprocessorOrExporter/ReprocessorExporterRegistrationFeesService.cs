using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ReprocessorOrExporter.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.ReprocessorOrExporter.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Services.RegistrationFees.ReprocessorOrExporter
{
    public class ReprocessorExporterRegistrationFeesService : IReprocessorExporterRegistrationFeesService
    {
        private readonly IHttpReprocessorExporterRegistrationFeesService _httpReprocessorExporterRegistrationFees;
        private readonly ILogger<ReprocessorExporterRegistrationFeesService> _logger;

        public ReprocessorExporterRegistrationFeesService(
            IHttpReprocessorExporterRegistrationFeesService httpReprocessorExporterRegistrationFeesService,
            ILogger<ReprocessorExporterRegistrationFeesService> logger)
        {
            _httpReprocessorExporterRegistrationFees = httpReprocessorExporterRegistrationFeesService ?? throw new ArgumentNullException(nameof(httpReprocessorExporterRegistrationFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public Task<ReprocessorOrExporterRegistrationFeesResponseDto?> CalculateFeesAsync(ReprocessorOrExporterRegistrationFeesRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErroreproExpoRegServiceFee);

            return CalculateFeesInternalAsync(request);
        }

        private async Task<ReprocessorOrExporterRegistrationFeesResponseDto?> CalculateFeesInternalAsync(ReprocessorOrExporterRegistrationFeesRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _httpReprocessorExporterRegistrationFees.CalculateFeesAsync(request, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateFeesAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErroreproExpoRegServiceFees);
                throw new ServiceException(ExceptionMessages.ErroreproExpoRegServiceFee, ex);
            }
        }
    }
}