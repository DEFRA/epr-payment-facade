using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;

namespace EPR.Payment.Facade.Services.RegistrationFees.Producer.Interfaces
{
    public interface IProducerFeesService
    {
        Task<ProducerFeesResponseDto> CalculateProducerFeesAsync(ProducerFeesRequestDto request, CancellationToken cancellationToken = default);

        Task<ProducerFeesResponseDto> CalculateProducerFeesAsync(ProducerFeesRequestV3Dto request, CancellationToken cancellationToken = default);
    }
}