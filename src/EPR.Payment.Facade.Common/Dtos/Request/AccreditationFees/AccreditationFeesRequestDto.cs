using EPR.Payment.Facade.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees
{
    public class AccreditationFeesRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RequestorTypes? RequestorType { get; set; }

        public string? Regulator { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TonnageBands? TonnageBand { get; set; }

        public int NumberOfOverseasSites { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MaterialTypes? MaterialType { get; set; }

        public string? ApplicationReferenceNumber { get; set; }

        public required DateTime SubmissionDate { get; set; }
    }
}
