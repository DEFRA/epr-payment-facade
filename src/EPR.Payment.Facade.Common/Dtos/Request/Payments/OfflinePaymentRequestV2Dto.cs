using EPR.Payment.Facade.Common.Enums.Payments;
using System.Text.Json.Serialization;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class OfflinePaymentRequestV2Dto : OfflinePaymentRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OfflinePaymentMethodTypes? PaymentMethod { get; set; }

        public Guid? OrganisationId { get; set; }
    }
}