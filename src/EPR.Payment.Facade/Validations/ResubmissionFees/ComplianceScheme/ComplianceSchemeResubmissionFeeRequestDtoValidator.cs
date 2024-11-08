using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Validations.Common;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.ResubmissionFees.ComplianceScheme
{
    public class ComplianceSchemeResubmissionFeeRequestDtoValidator : AbstractValidator<ComplianceSchemeResubmissionFeeRequestDto>
    {
        public ComplianceSchemeResubmissionFeeRequestDtoValidator()
        {
            RuleFor(x => x.Regulator)
                .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired)
                .Must(RegulatorValidationHelper.IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);

            RuleFor(x => x.ResubmissionDate)
                .Cascade(CascadeMode.Stop)
                .MustBeValidResubmissionDate();

            RuleFor(x => x.ReferenceNumber)
                .NotEmpty().WithMessage(ValidationMessages.ReferenceNumberRequired);

            RuleFor(x => x.MemberCount)
                .GreaterThan(0).WithMessage(ValidationMessages.MemberCountGreaterThanZero);
        }
    }
}