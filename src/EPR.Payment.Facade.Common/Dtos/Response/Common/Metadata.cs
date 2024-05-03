using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Response.Common
{
    public class Metadata
    {
        [JsonProperty("ledger_code")]
        public string? LedgerCode { get; set; }

        [JsonProperty("internal_reference_number")]
        public int InternalReferenceNumber { get; set; }
    }

}
