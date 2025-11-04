using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpOnlinePaymentsV2Service
    {
        Task<Guid> InsertOnlinePaymentAsync(InsertOnlinePaymentRequestV2Dto onlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default);
    }
}