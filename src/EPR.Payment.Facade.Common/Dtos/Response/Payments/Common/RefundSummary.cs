using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments.Common
{
    [ExcludeFromCodeCoverage]
    public class RefundSummary
    {
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("amount_available")]
        public int AmountAvailable { get; set; }

        [JsonProperty("amount_submitted")]
        public int AmountSubmitted { get; set; }
    }

}
