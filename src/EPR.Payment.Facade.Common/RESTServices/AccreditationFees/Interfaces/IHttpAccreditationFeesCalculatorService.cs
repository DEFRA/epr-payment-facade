using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;

namespace EPR.Payment.Facade.Common.RESTServices.AccreditationFees.Interfaces
{
    public interface IHttpAccreditationFeesCalculatorService
    {
        Task<ReprocessorOrExporterAccreditationFeesResponseDto?> CalculateAccreditationFeesAsync(
                 ReprocessorOrExporterAccreditationFeesRequestDto accreditationFeesRequestDto,
                 CancellationToken cancellationToken);
    }
}
