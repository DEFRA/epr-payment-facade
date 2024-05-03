using EPR.Payment.Facade.Common.Dtos.Response.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Response
{    
    public class PaymentResponseDto
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("metadata")]
        public Metadata? Metadata { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("state")]
        public State? State { get; set; }

        [JsonProperty("payment_id")]
        public string? PaymentId { get; set; }

        [JsonProperty("payment_provider")]
        public string? PaymentProvider { get; set; }

        [JsonProperty("created_date")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("refund_summary")]
        public RefundSummary? RefundSummary { get; set; }

        [JsonProperty("settlement_summary")]
        public SettlementSummary? SettlementSummary { get; set; }

        [JsonProperty("card_details")]
        public CardDetails? CardDetails { get; set; }

        [JsonProperty("delayed_capture")]
        public bool DelayedCapture { get; set; }

        [JsonProperty("moto")]
        public bool Moto { get; set; }

        [JsonProperty("return_url")]
        public string? ReturnUrl { get; set; }

        [JsonProperty("authorisation_mode")]
        public string? AuthorisationMode { get; set; }

        [JsonProperty("_links")]
        public Links? Links { get; set; }
    }

}
