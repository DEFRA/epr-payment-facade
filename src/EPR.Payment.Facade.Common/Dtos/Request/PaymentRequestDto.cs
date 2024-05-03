using EPR.Payment.Facade.Common.Dtos.Response.Common;
using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.Dtos.Request
{
    public class PaymentRequestDto
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }
        
        public string? return_url { get; set; }
    }
}
