using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.V2Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpOnlinePaymentsV2Service
    {
        Task<Guid> InsertOnlinePaymentAsync(InsertOnlinePaymentRequestV2Dto onlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default);
        Task UpdateOnlinePaymentAsync(Guid id, UpdateOnlinePaymentRequestV2Dto onlinePaymentStatusUpdateRequest, CancellationToken cancellationToken = default);
        Task<OnlinePaymentDetailsV2Dto> GetOnlinePaymentDetailsV2Async(Guid externalPaymentId, CancellationToken cancellationToken = default);
    }
}