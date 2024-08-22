using Swashbuckle.AspNetCore.Filters;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFee;
using EPR.Payment.Facade.Common.Enums.RegistrationFee;

namespace EPR.Payment.Facade.Examples;

/// <summary>
/// Payment Request Dto.
/// </summary>
public class CalculateRegistrationFeeRequestExample : IExamplesProvider<CalculateRegistrationFeeRequestDto>
{
    /// <summary>
    /// Example generator.
    /// </summary>
    /// <returns></returns>
    public CalculateRegistrationFeeRequestDto GetExamples()
    {
        return new CalculateRegistrationFeeRequestDto
        {
            IsOnlineMarketplace = true,
            NumberOfSubsidiaries = 210,
            ProducerType = "Large",
            Regulator = "GB-ENG"
        };
    }
}