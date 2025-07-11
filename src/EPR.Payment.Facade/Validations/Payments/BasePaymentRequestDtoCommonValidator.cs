using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class BasePaymentRequestDtoCommonValidator<T> : AbstractValidator<T> where T : BasePaymentRequestDto
    {
        public BasePaymentRequestDtoCommonValidator(bool isAccreditationFee = false)
        {
            RuleFor(x => x.UserId)
               .NotEmpty()
               .WithMessage(ValidationMessages.UserIdRequired);        

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(ValidationMessages.ReferenceRequired);

            RuleFor(x => x.Amount)
                .NotNull()
                .WithMessage(ValidationMessages.AmountRequired);                

            if (isAccreditationFee)
            {
                RuleFor(x => x.Description)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage(ValidationMessages.DescriptionRequired)
               .Must(text => text == PaymentDescConstants.RegistrationFee || text == PaymentDescConstants.PackagingResubmissionFee || text == PaymentDescConstants.AccreditationFee)
               .WithMessage(ValidationMessages.InvalidDescriptionV2);
            }
            else
            {
                RuleFor(x => x.Description)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage(ValidationMessages.DescriptionRequired)
               .Must(text => text == PaymentDescConstants.RegistrationFee || text == PaymentDescConstants.PackagingResubmissionFee)
               .WithMessage(ValidationMessages.InvalidDescription);
            }           
        }
    }
}