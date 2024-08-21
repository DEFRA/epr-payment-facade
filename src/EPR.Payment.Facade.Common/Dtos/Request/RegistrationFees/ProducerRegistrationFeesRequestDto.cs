namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees
{
    public class ProducerRegistrationFeesRequestDto
    {
        public required string ProducerType { get; set; } // "L" for Large, "S" for Small

        public int NumberOfSubsidiaries { get; set; } // Any integer >= 0

        public required string Regulator { get; set; } // "GB-ENG", "GB-SCT", etc.

        public bool IsOnlineMarketplace { get; set; } // True or False
        public bool PayBaseFee { get; set; } // Indicates whether the base fee should be paid
    }
}
