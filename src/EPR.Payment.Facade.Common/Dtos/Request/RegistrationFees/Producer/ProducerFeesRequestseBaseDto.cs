namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer
{
    public class ProducerFeesRequestseBaseDto
    {
        public required string ApplicationReferenceNumber { get; set; }

        public DateTime SubmissionDate { get; set; }

        public required string ProducerType { get; set; } // "large" or "small", case insensitive, cannot be empty

        public required string Regulator { get; set; } // "GB-ENG", "GB-SCT", etc.

        public int NumberOfSubsidiaries { get; set; } // Any integer >= 0

        public int NoOfSubsidiariesOnlineMarketplace { get; set; } // Any integer >= 0
    }
}