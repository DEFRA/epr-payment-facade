using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments
{
    public class GovPayResponseDto
    {
        [JsonProperty("payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("return_url")]
        public string ReturnUrl { get; set; }

        [JsonProperty("state")]
        public PaymentState State { get; set; }
    }

    public class PaymentState
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("finished")]
        public bool Finished { get; set; }
    }
}
