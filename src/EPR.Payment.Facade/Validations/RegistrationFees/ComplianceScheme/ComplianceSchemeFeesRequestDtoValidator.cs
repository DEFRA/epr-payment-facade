﻿using EPR.Payment.Facade.Common.Constants;
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

            RuleFor(x => x.SubmissionDate)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage(ValidationMessages.InvalidSubmissionDate)
                    .Must(BeInUtc).WithMessage(ValidationMessages.SubmissionDateMustBeUtc)
                    .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(ValidationMessages.FutureSubmissionDate);

            RuleForEach(x => x.ComplianceSchemeMembers)
            .SetValidator(new ComplianceSchemeMemberDtoValidator())
            .WithMessage(ValidationMessages.InvalidComplianceSchemeMember);
        }
        private static bool BeInUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Utc;
        }
    }
}
