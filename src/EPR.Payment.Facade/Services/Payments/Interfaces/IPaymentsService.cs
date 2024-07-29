using EPR.Payment.Facade.Common.Dtos.Request.Payments;

public interface IPaymentsService
{
    Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);
    Task CompletePaymentAsync(string govPayPaymentId, CompletePaymentRequestDto completeRequest, CancellationToken cancellationToken = default);
}
