using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments
{
    public class PaymentResponseDto
    {
        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("return_url")]
        public string ReturnUrl { get; set; }
    }
}
