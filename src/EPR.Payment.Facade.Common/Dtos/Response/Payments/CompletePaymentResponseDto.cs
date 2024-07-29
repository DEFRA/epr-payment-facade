using EPR.Payment.Facade.Common.Enums;

namespace EPR.Payment.Facade.Common.Dtos.Response.Payments
{
    public class CompletePaymentResponseDto
    {
        public PaymentStatus Status { get; set; }
        public string? Message { get; set; }
    }

}
