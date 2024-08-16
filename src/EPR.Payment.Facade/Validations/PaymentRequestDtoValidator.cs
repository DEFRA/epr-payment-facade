using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations
{
    public class PaymentRequestDtoValidator : AbstractValidator<PaymentRequestDto>
    {
        private const string InvalidUserIdErrorMessage = "User ID is required.";
        private const string InvalidOrganisationIdErrorMessage = "Organisation ID is required.";
        private const string InvalidReferenceErrorMessage = "Reference is required.";
        private const string InvalidAmountErrorMessage = "Amount is required and must be greater than zero.";
        private const string InvalidRegulatorNullErrorMessage = "Regulator is required.";
        private const string InvalidRegulatorErrorMessage = "Invalid Regulator.";
        private const string InvalidRegulatorNotENGErrorMessage = "Online payment is not supported for this regulator.";
        public PaymentRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotNull()
                .WithMessage(InvalidUserIdErrorMessage);
            RuleFor(x => x.OrganisationId)
                .NotNull()
                .WithMessage(InvalidOrganisationIdErrorMessage);
            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(InvalidReferenceErrorMessage);
            RuleFor(x => x.Amount)
                .NotNull()
                .GreaterThan(0)
                .WithMessage(InvalidAmountErrorMessage);
            RuleFor(x => x.Regulator)
                .NotEmpty()
                .WithMessage(InvalidRegulatorNullErrorMessage);
            RuleFor(x => x.Regulator)
                .Must(text => text == RegulatorConstants.GBENG || text == RegulatorConstants.GBSCT || text == RegulatorConstants.GBWLS || text == RegulatorConstants.GBNIR)
                .WithMessage(InvalidRegulatorErrorMessage);
            RuleFor(x => x.Regulator)
                .Must(text => text == RegulatorConstants.GBENG)
                .WithMessage(InvalidRegulatorNotENGErrorMessage)
                .When(x=> string.Equals(x.Regulator, RegulatorConstants.GBSCT, StringComparison.Ordinal) 
                       || string.Equals(x.Regulator, RegulatorConstants.GBWLS, StringComparison.Ordinal) 
                       || string.Equals(x.Regulator, RegulatorConstants.GBNIR, StringComparison.Ordinal));
        }
    }

}
