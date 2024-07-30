namespace EPR.Payment.Facade.Common.Dtos.Response.Payments
{
    public class PaymentDetailsDto
    {
        public Guid ExternalPaymentId { get; set; }
        public string? GovPayPaymentId { get; set; }
        public Guid UserId { get; set; }
        public Guid OrganisationId { get; set; }
        public string? Regulator { get; set; }
        public int Amount { get; set; }
    }
}
