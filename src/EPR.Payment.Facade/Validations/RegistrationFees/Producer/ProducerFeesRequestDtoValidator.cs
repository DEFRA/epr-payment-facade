using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Validations.Common;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.RegistrationFees.Producer
{
    public class ProducerFeesRequestDtoValidator : AbstractValidator<ProducerFeesRequestDto>
    {
        public ProducerFeesRequestDtoValidator()
        {
            var validProducerTypes = new List<string> { "LARGE", "SMALL" };

            RuleFor(x => x.ProducerType)
                .Must(pt => validProducerTypes.Contains(pt.ToUpper()))
                .WithMessage(ValidationMessages.ProducerTypeInvalid + string.Join(", ", validProducerTypes));

            RuleFor(x => x.NumberOfSubsidiaries)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.NumberOfSubsidiariesRange);

            RuleFor(x => x.NoOfSubsidiariesOnlineMarketplace)
                .LessThanOrEqualTo(x => x.NumberOfSubsidiaries).WithMessage(ValidationMessages.NumberOfOMPSubsidiariesLessThanOrEqualToNumberOfSubsidiaries);

            RuleFor(x => x.Regulator)
                .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired)
                .Must(RegulatorValidationHelper.IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);

            RuleFor(x => x.NoOfSubsidiariesOnlineMarketplace)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.NoOfSubsidiariesOnlineMarketplaceRange);

            RuleFor(x => x.ApplicationReferenceNumber)
                .NotEmpty().WithMessage(ValidationMessages.ApplicationReferenceNumberRequired);

            RuleFor(x => x.SubmissionDate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.InvalidSubmissionDate)
                .Must(BeInUtc).WithMessage(ValidationMessages.SubmissionDateMustBeUtc)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(ValidationMessages.FutureSubmissionDate);
        }
        private static bool BeInUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Utc;
        }
    }
}