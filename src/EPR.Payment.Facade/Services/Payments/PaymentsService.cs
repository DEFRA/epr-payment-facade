using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

public class PaymentsService : IPaymentsService
{
    private readonly IHttpGovPayService _httpGovPayService;
    private readonly IHttpPaymentsService _httpPaymentsService;
    private readonly ILogger<PaymentsService> _logger;
    private readonly PaymentServiceOptions _paymentServiceOptions;

    public PaymentsService(
        IHttpGovPayService httpGovPayService,
        IHttpPaymentsService httpPaymentsService,
        ILogger<PaymentsService> logger,
        IOptions<PaymentServiceOptions> paymentServiceOptions)
    {
        _httpGovPayService = httpGovPayService ?? throw new ArgumentNullException(nameof(httpGovPayService));
        _httpPaymentsService = httpPaymentsService ?? throw new ArgumentNullException(nameof(httpPaymentsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _paymentServiceOptions = paymentServiceOptions.Value ?? throw new ArgumentNullException(nameof(paymentServiceOptions));
    }

    public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto? request)
    {
        ValidateObject(request);

        // Use the static values from configuration
        var returnUrl = _paymentServiceOptions.ReturnUrl ?? throw new InvalidOperationException("ReturnUrl is not configured.");
        var description = _paymentServiceOptions.Description ?? throw new InvalidOperationException("Description is not configured.");

        // Create the GovPayPaymentRequestDto
        var govPayRequest = new GovPayPaymentRequestDto
        {
            Amount = request!.Amount!.Value,
            Reference = request.Reference,
            return_url = returnUrl,
            Description = description,
            OrganisationId = request.OrganisationId!.Value,
            UserId = request.UserId!.Value,
            Regulator = request.Regulator
        };

        var externalPaymentId = await InsertPaymentAsync(request);

        var govPayResponse = await _httpGovPayService.InitiatePaymentAsync(govPayRequest);

        if (string.IsNullOrEmpty(govPayResponse.PaymentId))
        {
            throw new InvalidOperationException("GovPay response does not contain a valid PaymentId.");
        }

        await UpdatePaymentAsync(externalPaymentId, request, govPayResponse.PaymentId, PaymentStatus.InProgress);

        return new PaymentResponseDto
        {
            NextUrl = govPayResponse.Links?.NextUrl?.Href
        };
    }

    public async Task CompletePaymentAsync(string? govPayPaymentId, CompletePaymentRequestDto completeRequest)
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
            Id = completeRequest.Id,
            GovPayPaymentId = govPayPaymentId,
            UpdatedByUserId = completeRequest.UpdatedByUserId,
            UpdatedByOrganisationId = completeRequest.UpdatedByOrganisationId,
            Reference = paymentStatusResponse.Reference,
            Status = status.Value,
            ErrorCode = paymentStatusResponse.State.Code
        };

        try
        {
            await _httpPaymentsService.UpdatePaymentAsync(completeRequest.Id, updateRequest);
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

    private async Task<Guid> InsertPaymentAsync(PaymentRequestDto request)
    {
        var insertRequest = new InsertPaymentRequestDto
        {
            UserId = request.UserId!.Value,
            OrganisationId = request.OrganisationId!.Value,
            Reference = request.Reference,
            Regulator = request.Regulator,
            Amount = request.Amount!.Value,
            ReasonForPayment = _paymentServiceOptions.Description,
            Status = PaymentStatus.Initiated
        };

        try
        {
            return await _httpPaymentsService.InsertPaymentAsync(insertRequest);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while inserting payment.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while inserting payment.");
            throw new Exception("An unexpected error occurred while inserting the payment.", ex);
        }
    }

    private async Task UpdatePaymentAsync(Guid id, PaymentRequestDto request, string paymentId, PaymentStatus status)
    {
        var updateRequest = new UpdatePaymentRequestDto
        {
            Id = id,
            GovPayPaymentId = paymentId,
            UpdatedByUserId = request.UserId!.Value,
            UpdatedByOrganisationId = request.OrganisationId!.Value,
            Reference = request.Reference,
            Status = status
        };

        try
        {
            await _httpPaymentsService.UpdatePaymentAsync(id, updateRequest);
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

    private void ValidateObject(object? obj)
    {
        var context = new ValidationContext(obj!, serviceProvider: null, items: null);
        Validator.ValidateObject(obj!, context, validateAllProperties: true);
    }
}
