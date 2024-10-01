﻿using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.RegistrationFees
{
    public class ProducerFeesRequestDtoValidator : AbstractValidator<ProducerFeesRequestDto>
    {
        public ProducerFeesRequestDtoValidator()
        {
            var validProducerTypes = new List<string> { "LARGE", "SMALL" };

            RuleFor(x => x.ProducerType)
                .Must(pt => string.IsNullOrEmpty(pt) || validProducerTypes.Contains(pt.ToUpper()))
                .WithMessage(ValidationMessages.ProducerTypeInvalid + string.Join(", ", validProducerTypes));

            RuleFor(x => x.NumberOfSubsidiaries)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.NumberOfSubsidiariesRange);

            RuleFor(x => x.NumberOfSubsidiaries)
                .GreaterThan(0)
                .When(x => string.IsNullOrEmpty(x.ProducerType))
                .WithMessage(ValidationMessages.NumberOfSubsidiariesRequiredWhenProducerTypeEmpty);

            RuleFor(x => x.Regulator)
                .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired)
                .Must(RegulatorValidationHelper.IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);

            RuleFor(x => x.NoOfSubsidiariesOnlineMarketplace)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.NoOfSubsidiariesOnlineMarketplaceRange);

            RuleFor(x => x.ApplicationReferenceNumber)
                .NotEmpty().WithMessage(ValidationMessages.ApplicationReferenceNumberRequired);
        }
    }
}