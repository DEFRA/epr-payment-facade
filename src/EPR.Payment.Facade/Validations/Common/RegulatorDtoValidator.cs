using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Common
{
    public class RegulatorDtoValidator : AbstractValidator<RegulatorDto>
    {
        public RegulatorDtoValidator()
        {
            RuleFor(x => x.Regulator)
                .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired)
                .Must(RegulatorValidationHelper.IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);
        }
    }
}