using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Enums.Payments;
using EPR.Payment.Facade.Common.Validators;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class GovPayRequestV2Dto : GovPayRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentsRequestorTypes? RequestorType { get; set; }
    }
}
