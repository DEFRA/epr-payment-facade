using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Response.Common
{
    public class Params
    {
        [JsonProperty("chargeTokenId")]
        public string? ChargeTokenId { get; set; }
    }

}
