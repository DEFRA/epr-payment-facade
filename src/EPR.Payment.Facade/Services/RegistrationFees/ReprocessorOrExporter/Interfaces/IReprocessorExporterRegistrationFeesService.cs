using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;

namespace EPR.Payment.Facade.Services.RegistrationFees.ReprocessorOrExporter.Interfaces
{
    public interface IReprocessorExporterRegistrationFeesService
    {
        Task<ReprocessorOrExporterRegistrationFeesResponseDto> CalculateFeesAsync(ReprocessorOrExporterRegistrationFeesRequestDto request, CancellationToken cancellationToken = default);
    }
}