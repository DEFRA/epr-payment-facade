using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpGovPayService
    {
        Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto requestDto);
        Task<PaymentStatusResponseDto> GetPaymentStatusAsync(string paymentId);
    }
}
