using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;

namespace EPR.Payment.Facade.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<GetFeeResponseDto> GetFee(bool isLarge, string regulator);
        Task<PaymentResponseDto> InitiatePayment(PaymentRequestDto request);
        Task<PaymentStatusResponseDto> GetPaymentStatus(string paymentId);
        Task InsertPaymentStatus(string paymentId, PaymentStatusInsertRequestDto paymentStatusInsertRequest);
    }
}