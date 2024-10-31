using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpOfflinePaymentsService
    {
        Task InsertOfflinePaymentAsync(OfflinePaymentRequestDto offlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default);
    }
}