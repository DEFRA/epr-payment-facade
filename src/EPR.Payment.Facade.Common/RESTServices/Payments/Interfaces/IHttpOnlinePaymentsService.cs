using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpOnlinePaymentsService
    {
        Task<Guid> InsertOnlinePaymentAsync(InsertOnlinePaymentRequestDto onlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default);
        Task UpdateOnlinePaymentAsync(Guid id, UpdateOnlinePaymentRequestDto onlinePaymentStatusUpdateRequest, CancellationToken cancellationToken = default);
        Task<OnlinePaymentDetailsDto> GetOnlinePaymentDetailsAsync(Guid externalPaymentId, CancellationToken cancellationToken = default);
    }
}
