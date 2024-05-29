using System.ComponentModel.DataAnnotations;

public class RegistrationFeeRequest
{
    [Required(ErrorMessage = "The 'Subsidiaries' field is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "The 'Subsidiaries' field must be a non-negative integer.")]
    public int Subsidiaries { get; set; }

    public bool IsLate { get; set; }

    public bool IsResubmission { get; set; }

    public bool IsOnlineMarketplace { get; set; }
}
