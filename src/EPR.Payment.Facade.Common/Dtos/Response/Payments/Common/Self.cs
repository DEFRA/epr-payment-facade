using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments.Common
{
    public class Self
    {
        [JsonProperty("href")]
        public string? Href { get; set; }

        [JsonProperty("method")]
        public string? Method { get; set; }
    }

}
