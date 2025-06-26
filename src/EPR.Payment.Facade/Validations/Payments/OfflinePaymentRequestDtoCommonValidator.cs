using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class OfflinePaymentRequestDtoCommonValidator<T> : AbstractValidator<T> where T : OfflinePaymentRequestDto
    {
        public OfflinePaymentRequestDtoCommonValidator(bool isAccreditationFee = false)
        {
            RuleFor(x => x.UserId)
               .NotNull()
               .WithMessage(ValidationMessages.UserIdRequired);

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(ValidationMessages.ReferenceRequired);

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
                .WithMessage(ValidationMessages.RegulatorRequired)
                .Must(text => text == RegulatorConstants.GBENG || text == RegulatorConstants.GBSCT || text == RegulatorConstants.GBWLS || text == RegulatorConstants.GBNIR)
                .WithMessage(ValidationMessages.InvalidRegulatorOffline);
        }
    }
}