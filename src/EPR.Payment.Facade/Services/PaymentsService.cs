using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services.Interfaces;
using System.Drawing;

namespace EPR.Payment.Facade.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IHttpGovPayService _httpGovPayService;
        private readonly IHttpPaymentsService _httpPaymentsService;

        public PaymentsService(IHttpGovPayService httpGovPayService, IHttpPaymentsService httpPaymentsService)
        {
            _httpGovPayService = httpGovPayService ?? throw new ArgumentNullException(nameof(httpGovPayService));
            _httpPaymentsService = httpPaymentsService ?? throw new ArgumentNullException(nameof(httpPaymentsService));
        }

        public async Task<PaymentResponseDto> InitiatePayment(PaymentRequestDto request)
        {
            return await _httpGovPayService.InitiatePayment(request);
        }

        public async Task<PaymentStatusResponseDto> GetPaymentStatus(string paymentId)
        {
            return await _httpGovPayService.GetPaymentStatus(paymentId);
        }

        public async Task InsertPaymentStatus(string paymentId, PaymentStatusInsertRequestDto request)
        {
            await _httpPaymentsService.InsertPaymentStatus(paymentId, request);
        }
    }
}
