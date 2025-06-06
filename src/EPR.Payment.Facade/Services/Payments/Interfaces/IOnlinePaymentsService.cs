using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Services.Payments.Interfaces
{
    public interface IOnlinePaymentsService
    {
        Task<OnlinePaymentResponseDto> InitiateOnlinePaymentAsync(OnlinePaymentRequestDto request, CancellationToken cancellationToken = default);
        Task<OnlinePaymentResponseDto> InitiateOnlinePaymentV2Async(OnlinePaymentRequestV2Dto request, CancellationToken cancellationToken = default);
        Task<CompleteOnlinePaymentResponseDto> CompleteOnlinePaymentAsync(Guid externalPaymentId, CancellationToken cancellationToken = default);
    }
}