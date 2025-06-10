using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{

    public class OnlinePaymentRequestDtoV2Validator : OnlinePaymentRequestDtoCommonValidator<OnlinePaymentRequestV2Dto>
    {
        public OnlinePaymentRequestDtoV2Validator() : base(true)
        {
            RuleFor(x => x.RequestorType)
                 .NotNull()
                 .WithMessage(ValidationMessages.RequestorTypeInvalid);
        }
    }
}