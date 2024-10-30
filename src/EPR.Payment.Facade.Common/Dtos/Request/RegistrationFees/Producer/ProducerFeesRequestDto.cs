namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer
{
    public class ProducerFeesRequestDto
    {
        public required string ProducerType { get; set; } // "large" or "small", case insensitive, cannot be empty

        public int NumberOfSubsidiaries { get; set; } // Any integer >= 0

        public required string Regulator { get; set; } // "GB-ENG", "GB-SCT", etc.

        public int NoOfSubsidiariesOnlineMarketplace { get; set; } // Any integer >= 0

        public bool IsProducerOnlineMarketplace { get; set; } // True or False

        public bool IsLateFeeApplicable { get; set; } // True or False

        public required string ApplicationReferenceNumber { get; set; }
        public required DateTime SubmissionDate { get; set; }
    }
}