using EPR.Payment.Facade.Common.Enums;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments
{
    public class CompleteOnlinePaymentResponseDto
    {
        public PaymentStatus Status { get; set; }
        public string? Message { get; set; }
        public string? Reference { get; set; }
        public Guid? UserId { get; set; }
        public Guid? OrganisationId { get; set; }
        public string? Regulator { get; set; }
        public decimal Amount { get; set; } = 0;
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? RequestorType { get; set; }
    }
}
