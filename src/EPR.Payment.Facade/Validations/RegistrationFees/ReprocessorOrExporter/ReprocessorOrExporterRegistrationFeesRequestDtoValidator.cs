using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Validations.Common;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.RegistrationFees.ReprocessorOrExporter
{

    public class ReprocessorOrExporterRegistrationFeesRequestDtoValidator : AbstractValidator<ReprocessorOrExporterRegistrationFeesRequestDto>
    {
        public ReprocessorOrExporterRegistrationFeesRequestDtoValidator()
        {
            RuleFor(x => x.Regulator)
                     .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired)
                     .Must(RegulatorValidationHelper.IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);

            RuleFor(x => x.SubmissionDate)
                    .Cascade(CascadeMode.Stop)
                    .MustBeValidSubmissionDate();

            RuleFor(x => x.SubmissionDate)
            .NotEmpty().WithMessage(ValidationMessages.ReprocessorExporterDateRequired)
                .Must(BeInUtc).WithMessage(ValidationMessages.ResubmissionDateMustBeUtc)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(ValidationMessages.RexExFutureResubmissionDate);

            RuleFor(x => x.MaterialType)
                .NotEmpty().WithMessage(ValidationMessages.MaterialTypeInvalid)
                .Must(value => Enum.IsDefined(typeof(MaterialTypes), value));

            RuleFor(x => x.RequestorType)
                .NotEmpty().WithMessage(ValidationMessages.RequestorTypeInvalid)
                .Must(value => Enum.IsDefined(typeof(RequestorTypes), value));
        }

        private static bool BeInUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Utc;
        }
    }
}