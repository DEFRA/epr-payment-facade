using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpOfflinePaymentsService
    {
        Task<Guid> InsertOfflinePaymentAsync(InsertOfflinePaymentRequestDto offlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default);
    }
}
