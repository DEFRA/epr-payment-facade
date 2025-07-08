namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class OnlinePaymentRequestDto : BasePaymentRequestDto
    {
        public Guid? OrganisationId { get; set; }
    }
}
