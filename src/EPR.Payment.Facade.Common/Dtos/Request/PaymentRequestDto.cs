using EPR.Payment.Facade.Common.Dtos.Response.Common;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request
{
    public class PaymentRequestDto
    {
        [Required(ErrorMessage = "Amount is required")]
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [Required(ErrorMessage = "Reference is required")]
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [JsonProperty("description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Return URL is required")]
        public string return_url { get; set; }
    }
}
