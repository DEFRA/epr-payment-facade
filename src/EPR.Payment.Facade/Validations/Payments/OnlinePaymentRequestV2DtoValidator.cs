using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Enums.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class OnlinePaymentRequestDtoV2Validator : AbstractValidator<OnlinePaymentRequestV2Dto>
    {
        public OnlinePaymentRequestDtoV2Validator()
        {
            RuleFor(x => x.UserId)
                .NotNull()
                .WithMessage(ValidationMessages.UserIdRequired);

            RuleFor(x => x.OrganisationId)
                .NotNull()
                .WithMessage(ValidationMessages.OrganisationIdRequired);

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(ValidationMessages.ReferenceRequired);

            RuleFor(x => x.Amount)
                .NotNull()
                .GreaterThan(0)
                .WithMessage(ValidationMessages.AmountRequiredAndGreaterThanZero);

            RuleFor(x => x.Regulator)
                .NotEmpty()
                .WithMessage(ValidationMessages.RegulatorRequired);

            RuleFor(x => x.Regulator)
                .Must(text => text == RegulatorConstants.GBENG || text == RegulatorConstants.GBSCT || text == RegulatorConstants.GBWLS || text == RegulatorConstants.GBNIR)
                .WithMessage(ValidationMessages.RegulatorInvalid);

            RuleFor(x => x.Regulator)
                .Must(text => text == RegulatorConstants.GBENG)
                .WithMessage(ValidationMessages.RegulatorNotENG)
                .When(x => string.Equals(x.Regulator, RegulatorConstants.GBSCT, StringComparison.Ordinal)
                       || string.Equals(x.Regulator, RegulatorConstants.GBWLS, StringComparison.Ordinal)
                       || string.Equals(x.Regulator, RegulatorConstants.GBNIR, StringComparison.Ordinal));

            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidationMessages.DescriptionRequired)
                .Must(text => text == PaymentDescConstants.RegistrationFee || text == PaymentDescConstants.PackagingResubmissionFee)
                .WithMessage(ValidationMessages.InvalidDescription);

            RuleFor(x => x.RequestorType)
                .NotNull()
                .WithMessage(ValidationMessages.RequestorTypeInvalid);
        }
    }
}