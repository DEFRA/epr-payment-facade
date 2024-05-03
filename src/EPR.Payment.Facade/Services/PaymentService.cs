using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services.Interfaces;
using System.Drawing;

namespace EPR.Payment.Facade.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IHttpGovPayService _httpGovPayService;
        private readonly IHttpFeeService _httpFeeService;

        public PaymentService(IHttpGovPayService httpGovPayService, IHttpFeeService httpFeeService)
        {
            _httpGovPayService = httpGovPayService ?? throw new ArgumentNullException(nameof(httpGovPayService));
            _httpFeeService = httpFeeService ?? throw new ArgumentNullException(nameof(httpFeeService));
        }

        public async Task<GetFeeResponseDto> GetFee(bool isLarge, string regulator)
        {
            return await _httpFeeService.GetFee(isLarge, regulator);
        }

        public async Task<PaymentStatusResponseDto> GetPaymentStatus(string paymentId)
        {
            return await _httpGovPayService.GetPaymentStatus(paymentId);
        }

        public async Task<PaymentResponseDto> InitiatePayment(PaymentRequestDto request)
        {
            return await _httpGovPayService.InitiatePayment(request);
        }

        public async Task InsertPaymentStatus(string paymentId, PaymentStatusInsertRequestDto request)
        {
            await _httpFeeService.InsertPaymentStatus(paymentId, request);
        }
    }
}
