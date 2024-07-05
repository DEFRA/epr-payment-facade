using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

public interface IHttpGovPayService
{
    Task<PaymentResponseDto> InitiatePaymentAsync(GovPayPaymentRequestDto request);
    Task<PaymentStatusResponseDto> GetPaymentStatusAsync(string paymentId);
}
