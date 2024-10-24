using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class OfflinePaymentRequestDtoValidator : AbstractValidator<OfflinePaymentRequestDto>
    {
        public OfflinePaymentRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotNull()
                .WithMessage(ValidationMessages.OfflinePaymentUserIdRequired);

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(ValidationMessages.OfflinePaymentReferenceRequired);

            RuleFor(x => x.Amount)
            .NotNull()
            .WithMessage(ValidationMessages.OfflinePaymentReferenceRequired);
        }
    }
}