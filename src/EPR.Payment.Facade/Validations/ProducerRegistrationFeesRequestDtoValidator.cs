using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using FluentValidation;

namespace EPR.Payment.Facade.Validations
{
    public class ProducerRegistrationFeesRequestDtoValidator : AbstractValidator<ProducerRegistrationFeesRequestDto>
    {
        public ProducerRegistrationFeesRequestDtoValidator()
        {
            RuleFor(x => x.ProducerType)
                .Must(pt => string.IsNullOrEmpty(pt) || pt.ToUpper() == "L" || pt.ToUpper() == "S")
                .WithMessage(ValidationMessages.ProducerTypeInvalid);

            RuleFor(x => x.NumberOfSubsidiaries)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.NumberOfSubsidiariesRange)
                .LessThanOrEqualTo(100).WithMessage(ValidationMessages.NumberOfSubsidiariesRange);

            RuleFor(x => x.NumberOfSubsidiaries)
                .GreaterThan(0)
                .When(x => string.IsNullOrEmpty(x.ProducerType))
                .WithMessage("Number of subsidiaries must be greater than 0 when ProducerType is empty.");

            RuleFor(x => x.Regulator)
                .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired)
                .Must(IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);
        }

        private bool IsValidRegulator(string regulator)
        {
            return regulator == RegulatorConstants.GBENG ||
                   regulator == RegulatorConstants.GBSCT ||
                   regulator == RegulatorConstants.GBWLS ||
                   regulator == RegulatorConstants.GBNIR;
        }
    }
}
