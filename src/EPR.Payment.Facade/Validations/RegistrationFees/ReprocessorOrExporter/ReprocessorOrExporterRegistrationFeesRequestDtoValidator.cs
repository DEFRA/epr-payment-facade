using EPR.Payment.Facade.Common.Constants;
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

            RuleFor(x => x.RequestorType)
               .NotNull().WithMessage(ValidationMessages.EmptyRequestorType)
               .IsInEnum()
               .WithMessage(ValidationMessages.InvalidRequestorType + string.Join(",", Enum.GetNames(typeof(RequestorTypes))));

            RuleFor(x => x.MaterialType)
               .NotNull().WithMessage(ValidationMessages.EmptyMaterialType)
               .IsInEnum()
               .WithMessage(ValidationMessages.InvalidMaterialType + string.Join(",", Enum.GetNames(typeof(MaterialTypes))));
        }
    }
}