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
                .LessThan(DateTime.UtcNow).WithMessage(ValidationMessages.RexExFutureResubmissionDate);

            RuleFor(x => x.MaterialType)
                .NotEmpty().WithMessage(ValidationMessages.MaterialTypeInvalid);

            RuleFor(x => x.RequestorType)
                .NotEmpty().WithMessage(ValidationMessages.RequestorTypeInvalid);
        }
    }
}