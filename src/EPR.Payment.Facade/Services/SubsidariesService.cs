using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Services.Interfaces;

namespace EPR.Payment.Facade.Services
{
    public class SubsidariesService : IFeesService
    {
        private readonly ILogger<SubsidariesService> _logger;

        public SubsidariesService(ILogger<SubsidariesService> logger)
        {
            _logger = logger;
        }

        public async Task<GetFeesResponseDto> GetFeesAsync()
        {
            try
            {
                // Call to the external API or calculation logic here


                // Map Fee object to GetFeesResponseDto object
                var response = new GetFeesResponseDto
                {
                    //Large = fee.Large,
                    //Regulator = fee.Regulator,
                    //Amount = fee.Amount,
                    //EffectiveFrom = fee.EffectiveFrom,
                    //EffectiveTo = fee.EffectiveTo
                };

                _logger.LogInformation("Successfully retrieved fees for subsidaries.");
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fees for subsidaries.");
                throw;
            }
        }
    }
}
