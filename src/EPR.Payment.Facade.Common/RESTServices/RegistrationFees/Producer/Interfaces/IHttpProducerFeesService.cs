using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces
{
    public interface IHttpProducerFeesService
    {
        Task<ProducerFeesResponseDto> CalculateProducerFeesAsync(ProducerFeesRequestDto request, CancellationToken cancellationToken = default);
    }

    public interface IHttpProducerFeesV3Service
    {
        Task<ProducerFeesResponseDto> CalculateProducerFeesAsync(ProducerFeesRequestV3Dto request, CancellationToken cancellationToken = default);
    }
}