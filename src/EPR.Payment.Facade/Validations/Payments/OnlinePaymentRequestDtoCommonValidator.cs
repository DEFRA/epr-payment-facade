using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class OnlinePaymentRequestDtoCommonValidator<T> : BasePaymentRequestDtoCommonValidator<T> where T: OnlinePaymentRequestDto
    {
        public OnlinePaymentRequestDtoCommonValidator(bool isAccreditationFee) : base(isAccreditationFee)
        {               
            RuleFor(x => x.OrganisationId)
                .NotNull()
                .WithMessage(ValidationMessages.OrganisationIdRequired);

            RuleFor(x => x.Regulator)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ValidationMessages.RegulatorRequired)
            .Must(text => text == RegulatorConstants.GBENG)
            .WithMessage(ValidationMessages.RegulatorNotENG);

            RuleFor(x => x.Amount)              
               .GreaterThan(0)
               .WithMessage(ValidationMessages.AmountGreaterThanZero);
        }
    }
}