namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class OfflinePaymentRequestDto : BasePaymentRequestDto
    {
        public DateTime? PaymentDate { get; set; }

        public string? Comments { get; set; }
    }
}