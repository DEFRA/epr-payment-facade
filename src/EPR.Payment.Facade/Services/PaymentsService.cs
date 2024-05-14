using System;
using System.Threading.Tasks;
using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services.Interfaces;

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
            ValidatePaymentRequest(request);

            return await _httpGovPayService.InitiatePayment(request);
        }

        public async Task<PaymentStatusResponseDto> GetPaymentStatus(string paymentId)
        {
            ValidatePaymentId(paymentId);

            return await _httpGovPayService.GetPaymentStatus(paymentId);
        }

        public async Task InsertPaymentStatus(string paymentId, PaymentStatusInsertRequestDto request)
        {
            ValidatePaymentId(paymentId);
            ValidatePaymentStatusInsertRequest(request);

            await _httpPaymentsService.InsertPaymentStatus(paymentId, request);
        }

        private static void ValidatePaymentRequest(PaymentRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Reference))
                throw new ArgumentException("Reference cannot be null or empty.", nameof(request.Reference));

            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(request.Amount));

            if (string.IsNullOrEmpty(request.Description))
                throw new ArgumentException("Description cannot be null or empty.", nameof(request.Description));

            if (string.IsNullOrEmpty(request.return_url))
                throw new ArgumentException("Return URL cannot be null or empty.", nameof(request.return_url));
        }

        private static void ValidatePaymentId(string paymentId)
        {
            if (string.IsNullOrEmpty(paymentId))
                throw new ArgumentException("PaymentId cannot be null or empty.", nameof(paymentId));
        }

        private static void ValidatePaymentStatusInsertRequest(PaymentStatusInsertRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Status))
                throw new ArgumentException("Status cannot be null or empty.", nameof(request.Status));

            // Add more validation checks for other properties as needed
        }
    }
}
