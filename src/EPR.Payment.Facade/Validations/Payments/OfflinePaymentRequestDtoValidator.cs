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
               .WithMessage(ValidationMessages.UserIdRequired);

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(ValidationMessages.ReferenceRequired);

            RuleFor(x => x.Amount)
                .NotNull()
                .GreaterThan(0)
                .WithMessage(ValidationMessages.AmountRequiredAndGreaterThanZero);

            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidationMessages.DescriptionRequired)
                .Must(text => text == OfflinePayDescConstants.RegistrationFee || text == OfflinePayDescConstants.PackagingResubmissionFee)
                .WithMessage(ValidationMessages.InvalidDescription);

            RuleFor(x => x.Regulator)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidationMessages.RegulatorRequired)
                .Must(text => text == RegulatorConstants.GBENG || text == RegulatorConstants.GBSCT || text == RegulatorConstants.GBWLS || text == RegulatorConstants.GBNIR)
                .WithMessage(ValidationMessages.InvalidRegulatorOffline);
        }
    }
}