using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Services.Fees
{
    public class LargeProducerFeeCalculatorService : IFeeCalculatorService<RegistrationFeeRequest>
    {
        private readonly LargeProducerHttpService _httpService;
        private readonly ILogger<LargeProducerFeeCalculatorService> _logger;

        public LargeProducerFeeCalculatorService(
            LargeProducerHttpService httpService,
            ILogger<LargeProducerFeeCalculatorService> logger)
        {
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<FeeResponse> CalculateFeeAsync(RegistrationFeeRequest request)
        {
            try
            {
                return await _httpService.CalculateFeeAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while calling the external API.");
                throw;
            }
        }
    }
}
