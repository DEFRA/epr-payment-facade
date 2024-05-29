using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
