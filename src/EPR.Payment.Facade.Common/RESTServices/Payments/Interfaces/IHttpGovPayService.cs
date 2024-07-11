using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpGovPayService
    {
        Task<GovPayResponseDto> InitiatePaymentAsync(GovPayPaymentRequestDto paymentRequestDto, CancellationToken cancellationToken = default);
        Task<PaymentStatusResponseDto?> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken = default);
    }
}
