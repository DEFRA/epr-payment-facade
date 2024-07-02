using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments.Common
{
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
