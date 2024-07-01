using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Services
{
    public class FeesService : IFeesService
    {
        private readonly IHttpFeesService _httpFeesService;
        private readonly ILogger<FeesService> _logger;

        public FeesService(IHttpFeesService httpFeesService, ILogger<FeesService> logger)
        {
            _httpFeesService = httpFeesService;
            _logger = logger;
        }

        public async Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationRequestDto request)
        {
            if (request.NumberOfSubsidiaries > 100)
            {
                throw new ArgumentException("Number of subsidiaries cannot exceed 100.");
            }

            if (!request.PayBaseFee && request.NumberOfSubsidiaries == 0)
            {
                throw new ArgumentException("No valid fees requested.");
            }

            try
            {
                return await _httpFeesService.CalculateProducerFeesAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating producer fees");
                throw; // Re-throw the exception to let it bubble up
            }
        }

        public async Task<RegistrationFeeResponseDto> CalculateComplianceSchemeFeesAsync(ComplianceSchemeRegistrationRequestDto request)
        {
            foreach (var producer in request.Producers)
            {
                if (producer.NumberOfSubsidiaries < 0 || producer.NumberOfSubsidiaries > 100)
                {
                    throw new ArgumentException("Number of subsidiaries per producer must be between 0 and 100.");
                }

                if (producer.ProducerType != "L" && producer.ProducerType != "S")
                {
                    throw new ArgumentException("ProducerType must be 'L' for Large or 'S' for Small.");
                }
            }

            if (!request.PayComplianceSchemeBaseFee && !request.Producers.Any(p => p.PayBaseFee) && !request.Producers.Any(p => p.NumberOfSubsidiaries > 0))
            {
                throw new ArgumentException("No valid fees requested.");
            }

            try
            {
                return await _httpFeesService.CalculateComplianceSchemeFeesAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating compliance scheme fees");
                throw; // Re-throw the exception to let it bubble up
            }
        }
    }
}
