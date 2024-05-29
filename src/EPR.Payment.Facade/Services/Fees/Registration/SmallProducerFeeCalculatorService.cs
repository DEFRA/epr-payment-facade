using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;

namespace EPR.Payment.Facade.Services.Fees
{
    public class SmallProducerFeeCalculatorService : IFeeCalculatorService<RegistrationFeeRequest>
    {
        private readonly SmallProducerHttpService _httpService;
        private readonly ILogger<SmallProducerFeeCalculatorService> _logger;

        public SmallProducerFeeCalculatorService(
            SmallProducerHttpService httpService,
            ILogger<SmallProducerFeeCalculatorService> logger)
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
