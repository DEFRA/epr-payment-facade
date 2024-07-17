using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments.Common
{
    public class Params
    {
        [JsonProperty("chargeTokenId")]
        public string? ChargeTokenId { get; set; }
    }

}
