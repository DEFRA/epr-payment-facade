﻿using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.RegistrationFees.ComplianceScheme
{
    public class ComplianceSchemeMemberDtoValidator : AbstractValidator<ComplianceSchemeMemberDto>
    {
        public ComplianceSchemeMemberDtoValidator()
        {
            var validMemberTypes = new List<string> { "LARGE", "SMALL" };

            RuleFor(x => x.MemberId)
                .GreaterThan(0)
                .WithMessage(ValidationMessages.InvalidMemberId);

            RuleFor(x => x.MemberType)
                .NotEmpty()
                .WithMessage(ValidationMessages.MemberTypeRequired)
                .Must(pt => validMemberTypes.Contains(pt.ToUpper()))
                .WithMessage(ValidationMessages.InvalidMemberType + string.Join(", ", validMemberTypes));

            RuleFor(x => x.NumberOfSubsidiaries)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidationMessages.NumberOfSubsidiariesRange);

            RuleFor(x => x.NoOfSubsidiariesOnlineMarketplace)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidationMessages.NoOfSubsidiariesOnlineMarketplaceRange);
        }
    }
}
