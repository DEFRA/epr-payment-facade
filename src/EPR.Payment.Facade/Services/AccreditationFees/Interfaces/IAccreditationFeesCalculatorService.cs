using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;

namespace EPR.Payment.Facade.Services.AccreditationFees.Interfaces
{
    public interface IAccreditationFeesCalculatorService
    {
        Task<AccreditationFeesResponseDto?> CalculateAccreditationFeesAsync(
                 AccreditationFeesRequestDto accreditationFeesRequestDto,
                 CancellationToken cancellationToken);
    }
}
