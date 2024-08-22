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
                .Must(pt => string.IsNullOrEmpty(pt) || pt == "L" || pt == "S")
                .WithMessage(ValidationMessages.ProducerTypeInvalid);

            RuleFor(x => x.NumberOfSubsidiaries)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.NumberOfSubsidiariesRange)
                .LessThanOrEqualTo(100).WithMessage(ValidationMessages.NumberOfSubsidiariesRange);

            RuleFor(x => x.Regulator)
                .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired);
        }
    }
}