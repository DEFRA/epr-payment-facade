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
                .WithMessage(string.Format(InvalidUserIdErrorMessage, nameof(PaymentRequestDto.UserId)));
            RuleFor(x => x.OrganisationId)
                .NotNull()
                .WithMessage(string.Format(InvalidOrganisationIdErrorMessage, nameof(PaymentRequestDto.OrganisationId)));
            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(string.Format(InvalidReferenceErrorMessage, nameof(PaymentRequestDto.Reference)));
            RuleFor(x => x.Amount)
                .NotNull()
                .GreaterThan(0)
                .WithMessage(string.Format(InvalidAmountErrorMessage, nameof(PaymentRequestDto.Amount)));
            RuleFor(x => x.Regulator)
                .NotEmpty()
                .WithMessage(string.Format(InvalidRegulatorNullErrorMessage, nameof(PaymentRequestDto.Regulator)));
            RuleFor(x => x.Regulator)
                .Must(text => text == RegulatorConstants.GBENG || text == RegulatorConstants.GBSCT || text == RegulatorConstants.GBWLS || text == RegulatorConstants.GBNIR)
                .WithMessage(string.Format(InvalidRegulatorErrorMessage, nameof(PaymentRequestDto.Regulator)));
            RuleFor(x => x.Regulator)
                .Must(text => text == RegulatorConstants.GBENG)
                .WithMessage(string.Format(InvalidRegulatorNotENGErrorMessage, nameof(PaymentRequestDto.Regulator)))
                .When(x=> string.Equals(x.Regulator, RegulatorConstants.GBSCT, StringComparison.Ordinal) 
                       || string.Equals(x.Regulator, RegulatorConstants.GBWLS, StringComparison.Ordinal) 
                       || string.Equals(x.Regulator, RegulatorConstants.GBNIR, StringComparison.Ordinal));
        }
    }

}
