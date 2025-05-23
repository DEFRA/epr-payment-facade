namespace EPR.Payment.Facade.Common.Dtos.Response.Payments
{
    public class PreviousPaymentDetailResponseDto
    {
        public required string PaymentMode { get; set; } // Online or Offline

        public required string? PaymentMethod { get; set; }

        public required DateTime PaymentDate { get; set; }

        public required decimal PaymentAmount { get; set; }
    }
}