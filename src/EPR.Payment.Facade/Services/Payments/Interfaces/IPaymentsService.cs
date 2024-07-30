using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

public interface IPaymentsService
{
    Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request, HttpResponse response, CancellationToken cancellationToken = default);
    Task<CompletePaymentResponseDto> CompletePaymentAsync(string govPayPaymentId, CompletePaymentRequestDto completeRequest, CancellationToken cancellationToken = default);
}
