using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;

namespace EPR.Payment.Facade.Services.Interfaces
{
    public interface IPaymentsService
    {        
        Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request);
        Task<PaymentStatusResponseDto> GetPaymentStatusAsync(string paymentId);
        Task InsertPaymentStatusAsync(string paymentId, PaymentStatusInsertRequestDto paymentStatusInsertRequest);
    }
}