using EPR.Payment.Facade.Common.Dtos.Request.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpPaymentsService
    {
        Task<Guid> InsertPaymentAsync(InsertPaymentRequestDto paymentStatusInsertRequest, CancellationToken cancellationToken = default);
        Task UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto paymentStatusUpdateRequest, CancellationToken cancellationToken = default);
    }
}
