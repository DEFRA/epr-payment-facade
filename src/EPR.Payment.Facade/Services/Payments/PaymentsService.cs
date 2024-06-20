using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.Extensions.Logging;

namespace EPR.Payment.Facade.Services.Payments
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IHttpGovPayService _httpGovPayService;
        private readonly IHttpPaymentsService _httpPaymentsService;
        private readonly ILogger<PaymentsService> _logger;

        public PaymentsService(
            IHttpGovPayService httpGovPayService,
            IHttpPaymentsService httpPaymentsService,
            ILogger<PaymentsService> logger)
        {
            _httpGovPayService = httpGovPayService ?? throw new ArgumentNullException(nameof(httpGovPayService));
            _httpPaymentsService = httpPaymentsService ?? throw new ArgumentNullException(nameof(httpPaymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request)
        {
            ValidateObject(request);

            var insertRequest = new InsertPaymentRequestDto
            {
                UserId = request.UserId,
                OrganisationId = request.OrganisationId,
                ReferenceNumber = request.ReferenceNumber,
                Regulator = request.Regulator,
                Amount = request.Amount,
                ReasonForPayment = request.ReasonForPayment,
                Status = PaymentStatus.Initiated
            };

            var externalPaymentId = await _httpPaymentsService.InsertPaymentAsync(insertRequest);

            var paymentResponse = await _httpGovPayService.InitiatePaymentAsync(request);

            var updateRequest = new UpdatePaymentRequestDto
            {
                ExternalPaymentId = externalPaymentId,
                GovPayPaymentId = paymentResponse.PaymentId,
                UpdatedByUserId = request.UserId,
                UpdatedByOrganisationId = request.OrganisationId,
                ReferenceNumber = request.ReferenceNumber,
                Status = PaymentStatus.InProgress
            };

            ValidateObject(updateRequest);

            await _httpPaymentsService.UpdatePaymentAsync(externalPaymentId, updateRequest);

            return paymentResponse;
        }

        public async Task CompletePaymentAsync(string govPayPaymentId, CompletePaymentRequestDto completeRequest)
        {
            if (string.IsNullOrWhiteSpace(govPayPaymentId))
            {
                _logger.LogError("GovPayPaymentId is null or empty.");
                throw new ArgumentException("GovPayPaymentId cannot be null or empty");
            }

            ValidateObject(completeRequest);

            var paymentStatusResponse = await _httpGovPayService.GetPaymentStatusAsync(govPayPaymentId);

            if (paymentStatusResponse == null || paymentStatusResponse.State?.Status == null)
            {
                _logger.LogError("Payment status not found or status is not available for GovPayPaymentId: {govPayPaymentId}", govPayPaymentId);
                throw new Exception("Payment status not found or status is not available.");
            }

            var status = paymentStatusResponse.State.Status switch
            {
                "success" => PaymentStatus.Success,
                "failed" => PaymentStatus.Failed,
                "error" => PaymentStatus.Error,
                _ => throw new Exception("Payment status is not valid.")
            };

            var updateRequest = new UpdatePaymentRequestDto
            {
                ExternalPaymentId = completeRequest.ExternalPaymentId,
                GovPayPaymentId = govPayPaymentId,
                UpdatedByUserId = completeRequest.UpdatedByUserId,
                UpdatedByOrganisationId = completeRequest.UpdatedByOrganisationId,
                ReferenceNumber = paymentStatusResponse.Reference,
                Status = status,
                ErrorCode = paymentStatusResponse.State.Code
            };

            ValidateObject(updateRequest);

            await _httpPaymentsService.UpdatePaymentAsync(completeRequest.ExternalPaymentId, updateRequest);
        }

        private void ValidateObject(object obj)
        {
            var context = new ValidationContext(obj);
            Validator.ValidateObject(obj, context, validateAllProperties: true);
        }
    }
}
