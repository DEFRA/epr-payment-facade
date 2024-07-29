namespace EPR.Payment.Facade.Common.Dtos.Response.Payments
{
    public class PaymentResponseDto
    {
        public string? NextUrl { get; set; }
        public Guid ExternalPaymentId { get; set; }
        public string GovPayPaymentId { get; set; } = string.Empty;
    }
}
