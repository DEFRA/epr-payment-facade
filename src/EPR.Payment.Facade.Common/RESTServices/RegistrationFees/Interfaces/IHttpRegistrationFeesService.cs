using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces
{
    public interface IHttpRegistrationFeesService
    {
        Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationFeeRequestDto request, CancellationToken cancellationToken = default);
    }
}
