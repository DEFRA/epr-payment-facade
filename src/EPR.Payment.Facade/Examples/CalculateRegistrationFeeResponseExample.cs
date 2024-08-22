using Swashbuckle.AspNetCore.Filters;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFee;

namespace EPR.Payment.Facade.Examples;

/// <summary>
/// Producer registration fee example response.
/// </summary>
public class CalculateRegistrationFeeResponseExample : IExamplesProvider<CalculateRegistrationFeeResponseDto>
{

    /// <summary>
    /// Get examples.
    /// </summary>
    /// <returns></returns>
    public CalculateRegistrationFeeResponseDto GetExamples()
    {
        return new CalculateRegistrationFeeResponseDto
        {
            TotalRegistrationFee = 12100,
            LargeProducerRegistrationFee = 12000,
            SubsidiaryRegistrationFee = 20,
            SubsidiaryRegistrationFee2 = 30,
            SubsidiaryRegistrationFee3 = 40,
            OnlineMarketplaceFee = 10
        };
    }
}