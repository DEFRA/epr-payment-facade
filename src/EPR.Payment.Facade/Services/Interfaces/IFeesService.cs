using EPR.Payment.Facade.Common.Dtos;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Services.Interfaces
{
    public interface IFeesService
    {
        Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationRequestDto request);
        Task<RegistrationFeeResponseDto> CalculateComplianceSchemeFeesAsync(ComplianceSchemeRegistrationRequestDto request);
    }
}
