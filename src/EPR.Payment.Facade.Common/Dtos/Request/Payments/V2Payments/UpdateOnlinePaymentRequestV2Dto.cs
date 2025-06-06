using EPR.Payment.Facade.Common.Enums.Payments;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments
{
    public class UpdateOnlinePaymentRequestV2Dto : UpdateOnlinePaymentRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentsRequestorTypes? RequestorType { get; set; }
    }
}