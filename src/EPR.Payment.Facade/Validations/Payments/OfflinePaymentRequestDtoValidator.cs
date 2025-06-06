using EPR.Payment.Facade.Common.Dtos.Request.Payments;

namespace EPR.Payment.Facade.Validations.Payments 
{
    public class OfflinePaymentRequestDtoValidator :OfflinePaymentRequestDtoCommonValidator<OfflinePaymentRequestDto>
    {
        public OfflinePaymentRequestDtoValidator() : base(false)
        {
           
        }
    }
}