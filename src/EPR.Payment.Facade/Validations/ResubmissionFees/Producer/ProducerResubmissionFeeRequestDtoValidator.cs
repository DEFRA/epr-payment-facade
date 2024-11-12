using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Validations.Common;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.ResubmissionFees.Producer
{
    public class ProducerResubmissionFeeRequestDtoValidator : AbstractValidator<ProducerResubmissionFeeRequestDto>
    {
        public ProducerResubmissionFeeRequestDtoValidator()
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
        }

        private bool BeInUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Utc;
        }

        private bool BeValidDate(DateTime dateTime)
        {
            return dateTime != DateTime.MinValue && dateTime != default(DateTime);
        }
    }
}