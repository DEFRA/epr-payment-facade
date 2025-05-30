using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;

namespace EPR.Payment.Facade.Common.RESTServices.AccreditationFees.Interfaces
{
    public interface IHttpAccreditationFeesCalculatorService
    {
        Task<AccreditationFeesResponseDto?> CalculateAccreditationFeesAsync(
                 AccreditationFeesRequestDto accreditationFeesRequestDto,
                 CancellationToken cancellationToken);
    }
}
