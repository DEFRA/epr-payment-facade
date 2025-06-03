using System.Text.Json.Serialization;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Enums;

namespace EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees
{
    public class ReprocessorOrExporterAccreditationFeesRequestDto : ReprocessorOrExporterRegistrationFeesRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TonnageBands? TonnageBand { get; set; }

        public required int NumberOfOverseasSites { get; set; }
    }
}
