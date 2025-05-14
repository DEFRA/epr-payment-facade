namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class OfflinePaymentV2RequestDto : OfflinePaymentRequestDto
    {
        public required string PaymentMethod { get; set; }
    }
}