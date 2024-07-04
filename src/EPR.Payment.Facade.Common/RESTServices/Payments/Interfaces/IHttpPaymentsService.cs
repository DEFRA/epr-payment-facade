using EPR.Payment.Facade.Common.Dtos.Request.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpPaymentsService
    {
        Task<Guid> InsertPaymentAsync(InsertPaymentRequestDto paymentStatusInsertRequest);
        Task UpdatePaymentAsync(Guid Id, UpdatePaymentRequestDto paymentStatusUpdateRequest);
    }
}
