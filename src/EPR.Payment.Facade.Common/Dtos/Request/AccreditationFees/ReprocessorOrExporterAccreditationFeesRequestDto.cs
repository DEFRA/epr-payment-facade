using System.Text.Json.Serialization;
using EPR.Payment.Facade.Common.Enums;

namespace EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees
{
    public class ReprocessorOrExporterAccreditationFeesRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RequestorTypes? RequestorType { get; set; }

        public required string Regulator { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TonnageBands? TonnageBand { get; set; }

        public required int NumberOfOverseasSites { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MaterialTypes? MaterialType { get; set; }

        public string? ApplicationReferenceNumber { get; set; }

        public required DateTime SubmissionDate { get; set; }
    }
}
