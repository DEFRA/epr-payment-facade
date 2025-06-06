using EPR.Payment.Facade.Common.Dtos.Request.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpOfflinePaymentsServiceV2
    {        
        Task InsertOfflinePaymentAsync(OfflinePaymentRequestV2Dto offlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default);
    }
}