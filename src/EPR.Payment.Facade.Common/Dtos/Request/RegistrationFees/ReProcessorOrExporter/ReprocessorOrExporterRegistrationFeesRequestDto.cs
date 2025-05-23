using EPR.Payment.Facade.Common.Enums;
using System.Text.Json.Serialization;

namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter
{
    public class ReprocessorOrExporterRegistrationFeesRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RequestorTypes? RequestorType { get; set; } // "exporter" or "reprocessor", case insensitive, cannot be an empty string        

        public required string Regulator { get; set; }

        public required DateTime SubmissionDate { get; set; } // Date of submission.

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MaterialTypes? MaterialType { get; set; } // "Plastic", "Paper", "Metal", etc.

        public string? ApplicationReferenceNumber { get; set; } // Reference number for the application.
    }
}