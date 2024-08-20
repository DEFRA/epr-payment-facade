using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;

namespace EPR.Payment.Facade.Services.RegistrationFees.Interfaces
{
    public interface IRegistrationFeesService
    {
        Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationFeeRequestDto request);
    }
}
