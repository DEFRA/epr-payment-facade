using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees
{
    public class ProducerRegistrationFeeRequestDto
    {
        [Required(ErrorMessage = "ProducerType is required.")]
        [RegularExpression("^(L|S)$", ErrorMessage = "ProducerType must be 'L' for Large or 'S' for Small.")]
        public required string ProducerType { get; set; } // "L" for Large, "S" for Small

        [Range(0, 100, ErrorMessage = "Number of subsidiaries must be between 0 and 100.")]
        public int NumberOfSubsidiaries { get; set; } // Any integer >= 0

        [Required(ErrorMessage = "Regulator is required.")]
        public required string Regulator { get; set; } // "GB-ENG", "GB-SCT", etc.

        public bool IsOnlineMarketplace { get; set; } // True or False
        public bool PayBaseFee { get; set; } // Indicates whether the base fee should be paid
    }
}
