namespace EPR.Payment.Facade.Common.Dtos.Response.RegistrationFee;

/// <summary>
/// Response contract for Registration fee calculation request.
/// </summary>
public class CalculateRegistrationFeeResponseDto
{
    /// <summary>
    /// Total registration fee.
    /// Fee currently stored in pence on PayCal side.
    /// This needs be formatted into £s on caller side.
    /// </summary>
    public int TotalRegistrationFee { get; set; }

    /// <summary>
    /// Large Producer Registration Fee.
    /// </summary>
    public int LargeProducerRegistrationFee { get; set; }

    /// <summary>
    /// Subsidiary registration fee for 1-20 subsidiaries
    /// </summary>
    public int SubsidiaryRegistrationFee { get; set; }

    /// <summary>
    /// Subsidiary registration fee for 21-100 subsidiaries.
    /// </summary>
    public int SubsidiaryRegistrationFee2 { get; set; }

    /// <summary>
    /// Subsidiary registration fee for more than 100 subsidiaries.
    /// </summary>
    public int SubsidiaryRegistrationFee3 { get; set; }

    /// <summary>
    /// Online marketplace fee.
    /// </summary>
    public int OnlineMarketplaceFee { get; set; }
}