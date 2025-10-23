using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces
{
    public interface IHttpProducerFeesV2Service
    {
        Task<ProducerFeesResponseDto> CalculateProducerFeesAsync(ProducerFeesRequestV2Dto request, CancellationToken cancellationToken = default);
    }
}