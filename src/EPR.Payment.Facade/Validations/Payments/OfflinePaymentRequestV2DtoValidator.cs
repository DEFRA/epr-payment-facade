using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class OfflinePaymentRequestV2DtoValidator : OfflinePaymentRequestDtoCommonValidator<OfflinePaymentRequestV2Dto> 
    {
        public OfflinePaymentRequestV2DtoValidator() : base(true)
        {
            RuleFor(x => x.PaymentMethod)
              .NotNull()
              .WithMessage(ValidationMessages.OfflinePaymentMethodRequired);
        }
    }
}