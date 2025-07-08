using EPR.Payment.Facade.Common.Enums.Payments;
using System.Text.Json.Serialization;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments
{
    public class OnlinePaymentRequestV2Dto: OnlinePaymentRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentsRequestorTypes? RequestorType { get; set; }
    }
}