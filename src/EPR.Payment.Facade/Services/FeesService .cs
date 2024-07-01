using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Services
{
    public class FeesService : IFeesService
    {
        private readonly IHttpFeesService _httpFeesService;

        public FeesService(IHttpFeesService httpFeesService)
        {
            _httpFeesService = httpFeesService;
        }

        public async Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationRequestDto request)
        {
            if (request.NumberOfSubsidiaries > 100)
            {
                throw new ArgumentException("Number of subsidiaries cannot exceed 100.");
            }

            return await _httpFeesService.CalculateProducerFeesAsync(request);
        }

        public async Task<RegistrationFeeResponseDto> CalculateComplianceSchemeFeesAsync(ComplianceSchemeRegistrationRequestDto request)
        {
            foreach (var producer in request.Producers)
            {
                if (producer.NumberOfSubsidiaries > 100)
                {
                    throw new ArgumentException("Number of subsidiaries cannot exceed 100.");
                }
            }

            return await _httpFeesService.CalculateComplianceSchemeFeesAsync(request);
        }
    }

}
