using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Validations.Common;
using FluentValidation;

namespace EPR.Payment.Facade.Validations.ResubmissionFees.Producer
{
    public class ProducerResubmissionFeeRequestV2DtoValidator : AbstractValidator<ProducerResubmissionFeeRequestV2Dto>
    {
        public ProducerResubmissionFeeRequestV2DtoValidator()
        {
            RuleFor(x => x.Regulator)
                .NotEmpty().WithMessage(ValidationMessages.RegulatorRequired)
                .Must(RegulatorValidationHelper.IsValidRegulator).WithMessage(ValidationMessages.RegulatorInvalid);

            RuleFor(x => x.ResubmissionDate)
                 .Cascade(CascadeMode.Stop)
                 .MustBeValidResubmissionDate();

            RuleFor(x => x.ReferenceNumber)
                .NotEmpty().WithMessage(ValidationMessages.ReferenceNumberRequired);

            RuleFor(x => x.FileId)
                    .NotEmpty().WithMessage(ValidationMessages.FileIdRequired);

            RuleFor(x => x.ExternalId)
                    .NotEmpty().WithMessage(ValidationMessages.ExternalIdRequired);

            RuleFor(x => x.InvoicePeriod)
                    .NotEmpty().WithMessage(ValidationMessages.InvoicePeriodRequired);

            RuleFor(x => x.PayerTypeId)
                    .GreaterThan(0).WithMessage(ValidationMessages.PayerTypeIdRequired);

            RuleFor(x => x.PayerId)
                    .GreaterThan(0).WithMessage(ValidationMessages.PayerIdRequired);
        }
    }
}