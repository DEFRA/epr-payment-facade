using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using System.ComponentModel.DataAnnotations;

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
        _httpGovPayService = httpGovPayService;
        _httpPaymentsService = httpPaymentsService;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request)
    {
        ValidateObject(request);

        if (!request.UserId.HasValue || !request.OrganisationId.HasValue)
        {
            throw new ValidationException("User ID and Organisation ID must be provided.");
        }

        var insertRequest = new InsertPaymentRequestDto
        {
            UserId = request.UserId.Value,
            OrganisationId = request.OrganisationId.Value,
            Reference = request.Reference,
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
            UpdatedByUserId = request.UserId.Value,
            UpdatedByOrganisationId = request.OrganisationId.Value,
            Reference = request.Reference,
            Status = PaymentStatus.InProgress
        };

        try
        {
            await _httpPaymentsService.UpdatePaymentAsync(externalPaymentId, updateRequest);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while updating payment status.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating payment status.");
            throw new Exception("An unexpected error occurred while updating the payment status.", ex);
        }

        return paymentResponse;
    }

    public async Task CompletePaymentAsync(string govPayPaymentId, CompletePaymentRequestDto completeRequest)
    {
        if (string.IsNullOrEmpty(govPayPaymentId))
            throw new ArgumentException("GovPayPaymentId cannot be null or empty", nameof(govPayPaymentId));

        var paymentStatusResponse = await _httpGovPayService.GetPaymentStatusAsync(govPayPaymentId);
        if (paymentStatusResponse == null || paymentStatusResponse.State == null)
            throw new Exception("Payment status not found or status is not available.");

        PaymentStatus? status = paymentStatusResponse.State.Status switch
        {
            "success" => PaymentStatus.Success,
            "failed" => PaymentStatus.Failed,
            "error" => PaymentStatus.Error,
            _ => null
        };

        if (status == null)
            throw new Exception("Payment status not found or status is not available.");

        var updateRequest = new UpdatePaymentRequestDto
        {
            ExternalPaymentId = completeRequest.ExternalPaymentId,
            GovPayPaymentId = govPayPaymentId,
            UpdatedByUserId = completeRequest.UpdatedByUserId,
            UpdatedByOrganisationId = completeRequest.UpdatedByOrganisationId,
            Reference = paymentStatusResponse.Reference,
            Status = status.Value,
            ErrorCode = paymentStatusResponse.State.Code
        };

        try
        {
            await _httpPaymentsService.UpdatePaymentAsync(completeRequest.ExternalPaymentId, updateRequest);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while updating payment status.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating payment status.");
            throw new Exception("An unexpected error occurred while updating the payment status.", ex);
        }
    }

    private void ValidateObject(object obj)
    {
        var context = new ValidationContext(obj, serviceProvider: null, items: null);
        Validator.ValidateObject(obj, context, validateAllProperties: true);
    }
}
