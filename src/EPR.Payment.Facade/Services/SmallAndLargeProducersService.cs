using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Services.Interfaces;

namespace EPR.Payment.Facade.Services
{
    public class SmallAndLargeProducersService : IFeesService
    {
        private readonly ILogger<SmallAndLargeProducersService> _logger;

        public SmallAndLargeProducersService(ILogger<SmallAndLargeProducersService> logger)
        {
            _logger = logger;
        }

        public async Task<GetFeesResponseDto> GetFeesAsync()
        {
            try
            {
                // Call to the external API or calculation logic here

                // Map Fee object to GetFeesResponseDto object
                var response = new GetFeesResponseDto()
                {
                    //Large = fee.Large,
                    //Regulator = fee.Regulator,
                    //Amount = fee.Amount,
                    //EffectiveFrom = fee.EffectiveFrom,
                    //EffectiveTo = fee.EffectiveTo
                };

                _logger.LogInformation("Successfully retrieved fees for small and large producers.");
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fees for small and large producers.");
                throw;
            }
        }
    }   

}
