using EPR.Payment.Facade.Common.Dtos.Request.Payments;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class OnlinePaymentRequestDtoValidator : OnlinePaymentRequestDtoCommonValidator<OnlinePaymentRequestDto>
    {
        public OnlinePaymentRequestDtoValidator() : base(false)
        {

        }
    }
}