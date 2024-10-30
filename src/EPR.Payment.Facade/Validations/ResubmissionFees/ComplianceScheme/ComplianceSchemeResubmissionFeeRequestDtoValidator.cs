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
                .NotEmpty().WithMessage(ValidationMessages.ResubmissionDateRequired)
                .Must(BeInUtc).WithMessage(ValidationMessages.ResubmissionDateMustBeUtc)
                .Must(BeValidDate).WithMessage(ValidationMessages.ResubmissionDateDefaultInvalid)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(ValidationMessages.ResubmissionDateInvalid);

            RuleFor(x => x.ReferenceNumber)
                .NotEmpty().WithMessage(ValidationMessages.ReferenceNumberRequired);

            RuleFor(x => x.MemberCount)
                .GreaterThan(0).WithMessage(ValidationMessages.MemberCountGreaterThanZero);
        }

        private bool BeInUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Utc;
        }

        private bool BeValidDate(DateTime dateTime)
        {
            // Here we check if the date is valid and if it is in a reasonable range
            return dateTime != DateTime.MinValue && dateTime != default(DateTime);
        }
    }
}