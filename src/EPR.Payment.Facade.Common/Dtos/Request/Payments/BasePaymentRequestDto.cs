namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class BasePaymentRequestDto
    {
        public Guid? UserId { get; set; }      

        public string? Reference { get; set; }

        public string? Regulator { get; set; }

        public int? Amount { get; set; }

        public string? Description { get; set; }
    }
}
