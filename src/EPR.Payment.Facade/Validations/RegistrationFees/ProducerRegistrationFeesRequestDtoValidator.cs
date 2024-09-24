﻿using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.RegistrationFees
{
    public class ProducerRegistrationFeesRequestDtoValidator : AbstractValidator<ProducerRegistrationFeesRequestDto>
    {
        public ProducerRegistrationFeesRequestDtoValidator()
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
                .Must(IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);
        }

        private bool IsValidRegulator(string regulator)
        {
            var validRegulators = new List<string>
            {
                RegulatorConstants.GBENG,
                RegulatorConstants.GBSCT,
                RegulatorConstants.GBWLS,
                RegulatorConstants.GBNIR
            };

            return validRegulators.Contains(regulator);
        }
    }
}