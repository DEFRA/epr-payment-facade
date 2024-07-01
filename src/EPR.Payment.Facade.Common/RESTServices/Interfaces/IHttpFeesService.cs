using EPR.Payment.Facade.Common.Dtos;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Interfaces
{
    public interface IHttpFeesService
    {
        Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationRequestDto request);
        Task<RegistrationFeeResponseDto> CalculateComplianceSchemeFeesAsync(ComplianceSchemeRegistrationRequestDto request);
    }

}
