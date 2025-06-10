using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class OnlinePaymentRequestDtoCommonValidator<T> : AbstractValidator<T> where T : OnlinePaymentRequestDto
    {
        public OnlinePaymentRequestDtoCommonValidator(bool isAccreditationFee = false)
        {
            RuleFor(x => x.UserId)
               .NotNull()
               .WithMessage(ValidationMessages.UserIdRequired);

            RuleFor(x => x.OrganisationId)
               .NotNull()
               .WithMessage(ValidationMessages.OrganisationIdRequired);

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(ValidationMessages.OfflineReferenceRequired);

            RuleFor(x => x.Amount)
                .NotNull()
                .WithMessage(ValidationMessages.AmountRequiredAndGreaterThanZero);

            RuleFor(x => x.Amount)
             .GreaterThanOrEqualTo(1)
             .WithMessage("Amount must be at least 1.");

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

            RuleFor(x => x.Regulator)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidationMessages.OfflineRegulatorRequired)
                .Must(text => text == RegulatorConstants.GBENG || text == RegulatorConstants.GBSCT || text == RegulatorConstants.GBWLS || text == RegulatorConstants.GBNIR)
                .WithMessage(ValidationMessages.InvalidRegulatorOffline);
        }
    }
}