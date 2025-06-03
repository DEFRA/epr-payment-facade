using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Services.Payments.Interfaces
{
    public interface IOfflinePaymentsService
    {
        Task OfflinePaymentAsync(OfflinePaymentRequestDto request, CancellationToken cancellationToken = default);
        Task OfflinePaymentAsync(OfflinePaymentRequestV2Dto request, CancellationToken cancellationToken = default);
    }
}