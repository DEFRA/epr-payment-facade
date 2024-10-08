﻿using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments.Common
{
    public class State
    {
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("finished")]
        public bool Finished { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }
    }

}
