using EPR.Payment.Facade.Common.Enums.RegistrationFee;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFee;

/// <summary>
/// PayCal Registration fee calculator request. 
/// </summary>
public class CalculateRegistrationFeeRequestDto
{
    /// <summary>
    /// Producer type.
    /// Accepted values Small, Large and/or NullOrEmpty, 
    /// Permitted values are Small and Large producer types..
    /// Send an empty string when producer registration fees is not required i.e
    /// NullOREmpty when Large producer has already registered, and subsidiaries are registered late
    /// NullOREmpty when Small producer has already registered, and subsidiaries are registered later.
    /// </summary>
    public string? ProducerType { get; set; }

    /// <summary>
    /// Number of subsidiaries.
    /// </summary>
    [Required(ErrorMessage = $"{nameof(NumberOfSubsidiaries)} is required")]
    public int? NumberOfSubsidiaries { get; set; }

    /// <summary>
    /// Regulator.
    /// Accepted values - GB-ENG, GB-SCT,GB-WTL, GB-NIR.
    /// </summary>
    public string? Regulator { get; set; }

    /// <summary>
    /// Is Online Marketplace.
    /// True – when producer has indicated that it is also an online marketplace.
    /// False – when producer has indicated that it is not an online marketplace.
    /// NULL when Large producer has already registered, and subsidiaries are registered late
    /// NULL when Small producer has already registered, and subsidiaries are registered later.
    /// </summary>
    public bool IsOnlineMarketplace { get; set; }
}