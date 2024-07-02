using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments.Common
{
    public class Metadata
    {
        [JsonProperty("ledger_code")]
        public string? LedgerCode { get; set; }

        [JsonProperty("internal_reference_number")]
        public int InternalReferenceNumber { get; set; }
    }
}
