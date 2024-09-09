using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;

namespace EPR.Payment.Facade.Services.RegistrationFees.Interfaces
{
    public interface IRegistrationFeesService
    {
        Task<RegistrationFeesResponseDto> CalculateProducerFeesAsync(ProducerRegistrationFeesRequestDto request, CancellationToken cancellationToken = default);
        Task<decimal?> GetResubmissionFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default);
    }
}