using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Validations.Common;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.RegistrationFees.ComplianceScheme
{
    public class ComplianceSchemeFeesRequestDtoValidator : AbstractValidator<ComplianceSchemeFeesRequestDto>
    {
        public ComplianceSchemeFeesRequestDtoValidator()
        {
            RuleFor(x => x.Regulator)
                    .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired)
                    .Must(RegulatorValidationHelper.IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);

            RuleFor(x => x.ApplicationReferenceNumber)
                    .NotEmpty().WithMessage(ValidationMessages.ApplicationReferenceNumberRequired);

            RuleForEach(x => x.ComplianceSchemeMembers)
            .SetValidator(new ComplianceSchemeMemberDtoValidator())
            .WithMessage(ValidationMessages.InvalidComplianceSchemeMember);
        }
    }
}
