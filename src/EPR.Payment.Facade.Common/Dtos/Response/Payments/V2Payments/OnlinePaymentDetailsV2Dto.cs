using EPR.Payment.Facade.Common.Enums.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments.V2Payments
{
    public class OnlinePaymentDetailsV2Dto : OnlinePaymentDetailsDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentsRequestorTypes? RequestorType { get; set; }
    }
}