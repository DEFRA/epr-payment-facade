namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class OfflinePaymentRequestDto : BasePaymentRequestDto
    {
        public new Guid? FileId { get; set; }

        public string? RegistrationBlobName { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? Comments { get; set; }
    }
}