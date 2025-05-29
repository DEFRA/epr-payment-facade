using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ReprocessorOrExporter.Interfaces
{
    public interface IHttpReprocessorExporterRegistrationFeesService
    {
        Task<ReprocessorOrExporterRegistrationFeesResponseDto> CalculateFeesAsync(ReprocessorOrExporterRegistrationFeesRequestDto request, CancellationToken cancellationToken = default);
    }
}