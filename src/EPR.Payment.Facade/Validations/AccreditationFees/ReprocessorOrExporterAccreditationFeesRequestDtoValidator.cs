using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Validations.Common;
using EPR.Payment.Facade.Validations.RegistrationFees.ReprocessorOrExporter;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.AccreditationFees
{
    public class ReprocessorOrExporterAccreditationFeesRequestDtoValidator : ReprocessorOrExporterFeesRequestDtoCommonValidator<ReprocessorOrExporterAccreditationFeesRequestDto>
    {
        public ReprocessorOrExporterAccreditationFeesRequestDtoValidator()
        {
            RuleFor(x => x.TonnageBand)
                .NotNull().WithMessage(ValidationMessages.EmptyTonnageBand)
                .IsInEnum()
                .WithMessage(ValidationMessages.InvalidTonnageBand + string.Join(",", Enum.GetNames(typeof(TonnageBands))));
            
            RuleFor(x => x.NumberOfOverseasSites)
               .GreaterThan(0).When(x => x.RequestorType == RequestorTypes.Exporters)
               .LessThanOrEqualTo(ReprocessorExporterConstants.MaxNumberOfOverseasSitesAllowed).When(x => x.RequestorType == RequestorTypes.Exporters).WithMessage(ValidationMessages.InvalidNumberOfOverseasSiteForExporter);

            RuleFor(x => x.NumberOfOverseasSites)
               .Equal(0).When(x => x.RequestorType == RequestorTypes.Reprocessors).WithMessage(ValidationMessages.InvalidNumberOfOverseasSiteForReprocessor);
        }
    }
}
