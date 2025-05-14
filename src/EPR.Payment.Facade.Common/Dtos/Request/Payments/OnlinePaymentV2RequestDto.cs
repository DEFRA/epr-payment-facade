using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class OnlinePaymentV2RequestDto : OnlinePaymentRequestDto
    {
        public required string RequestorType { get; set; } // "ReProcessor" or "Exporter".
    }
}
