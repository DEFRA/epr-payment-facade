﻿using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.Payments
{
    public class OfflinePaymentRequestDtoValidator : AbstractValidator<OfflinePaymentRequestDto>
    {
        public OfflinePaymentRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
               .NotNull()
               .WithMessage(ValidationMessages.UserIdRequired);

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(ValidationMessages.ReferenceRequired);

            RuleFor(x => x.Amount)
                .NotNull()
                .GreaterThan(0)
                .WithMessage(ValidationMessages.AmountRequiredAndGreaterThanZero);

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage(ValidationMessages.DescriptionRequired);

            RuleFor(x => x.Regulator)
                .NotEmpty()
                .WithMessage(ValidationMessages.RegulatorRequired);

            RuleFor(x => x.Regulator)
                .Must(text => text == RegulatorConstants.GBENG || text == RegulatorConstants.GBSCT || text == RegulatorConstants.GBWLS || text == RegulatorConstants.GBNIR)
                .WithMessage(ValidationMessages.RegulatorInvalid);

            RuleFor(x => x.Regulator)
                .Must(text => text == RegulatorConstants.GBENG)
                .WithMessage(ValidationMessages.RegulatorNotENG)
                .When(x => string.Equals(x.Regulator, RegulatorConstants.GBSCT, StringComparison.Ordinal)
                       || string.Equals(x.Regulator, RegulatorConstants.GBWLS, StringComparison.Ordinal)
                       || string.Equals(x.Regulator, RegulatorConstants.GBNIR, StringComparison.Ordinal));
        }
    }
}