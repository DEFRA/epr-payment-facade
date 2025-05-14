namespace EPR.Payment.Facade.Common.Dtos.Request.AcceridiationFees.ReprocessorOrExporter
{
    public class ReprocessorOrExporterAcceridiationFeesRequestDto
    {
        public required string RequestorType { get; set; } // "Reprocessor" or "Exporter".

        public required string Regulator { get; set; } // "GB-ENG", "GB-SCT", etc.

        public required DateTime SubmissionDate { get; set; }

        public required int NumberOfOverseeSites { get; set; } // Only applicable to exporters 0 for Reprocessor and greator than 0 for exporter

        public required string MaterialType { get; set; } // "Plastic", "Paper", "Metal", etc.

        public required string TonnageBand { get; set; } // "Upto500", "Over500To1000", "Over1000To10000", "Over10000".

        public string? ApplicationReferenceNumber { get; set; } // Reference number for the application.
    }
}
