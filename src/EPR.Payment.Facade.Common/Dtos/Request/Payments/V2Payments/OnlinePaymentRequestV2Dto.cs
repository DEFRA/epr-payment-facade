using EPR.Payment.Facade.Common.Enums.Payments;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments
{
    public class OnlinePaymentRequestV2Dto: OnlinePaymentRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentsRequestorTypes? RequestorType { get; set; }
    }
}