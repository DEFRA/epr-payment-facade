using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpPaymentsService
    {
        Task<Guid> InsertPaymentAsync(InsertPaymentRequestDto paymentStatusInsertRequest, CancellationToken cancellationToken = default);
        Task UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto paymentStatusUpdateRequest, CancellationToken cancellationToken = default);
        Task<PaymentDetailsDto> GetPaymentDetailsAsync(Guid externalPaymentId, CancellationToken cancellationToken = default);
    }
}
