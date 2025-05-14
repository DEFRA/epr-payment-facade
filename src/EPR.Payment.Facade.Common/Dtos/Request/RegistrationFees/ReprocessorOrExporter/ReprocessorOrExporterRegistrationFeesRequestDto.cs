namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter
{
    public class ReprocessorOrExporterRegistrationFeesRequestDto
    {
        public required string RequestorType { get; set; } // "Reprocessor" or "Exporter".

        public required string Regulator { get; set; } // "GB-ENG", "GB-SCT", etc.

        public required DateTime SubmissionDate { get; set; } // Date of submission.

        public required string MaterialType { get; set; } // "Plastic", "Paper", "Metal", etc.

        public string? ApplicationReferenceNumber { get; set; } // Reference number for the application.
    }
}
