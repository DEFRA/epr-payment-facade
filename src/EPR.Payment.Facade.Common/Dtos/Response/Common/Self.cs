﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Response.Common
{
    public class Self
    {
        [JsonProperty("href")]
        public string? Href { get; set; }

        [JsonProperty("method")]
        public string? Method { get; set; }
    }

}
