using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
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

    public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request)
    {
        ValidateObject(request);

        if (!request.UserId.HasValue || !request.OrganisationId.HasValue)
        {
            throw new ValidationException("User ID and Organisation ID must be provided.");
        }

        // Use the static values from configuration
        var returnUrl = _paymentServiceOptions.ReturnUrl ?? throw new InvalidOperationException("ReturnUrl is not configured.");
        var description = _paymentServiceOptions.Description ?? throw new InvalidOperationException("Description is not configured.");

        // Add returnUrl and description to the request
        var govPayRequest = new GovPayPaymentRequestDto
        {
            Amount = request.Amount,
            Reference = request.Reference,
            return_url = returnUrl,
            UserId = request.UserId.Value,
            OrganisationId = request.OrganisationId.Value,
            Regulator = request.Regulator,
            Description = description
        };

        var id = await InsertPaymentAsync(request);
        var paymentResponse = await _httpGovPayService.InitiatePaymentAsync(govPayRequest);
        await UpdatePaymentAsync(id, request, paymentResponse.PaymentId, PaymentStatus.InProgress);

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
            UserId = request.UserId.Value,
            OrganisationId = request.OrganisationId.Value,
            Reference = request.Reference,
            Regulator = request.Regulator,
            Amount = request.Amount,
            ReasonForPayment = _paymentServiceOptions.Description, // Use the configured description
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

    private async Task<PaymentResponseDto> InitiateGovPayPaymentAsync(GovPayPaymentRequestDto request)
    {
        try
        {
            return await _httpGovPayService.InitiatePaymentAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while initiating payment.");
            throw new Exception("An unexpected error occurred while initiating the payment.", ex);
        }
    }

    private async Task UpdatePaymentAsync(Guid id, PaymentRequestDto request, string paymentId, PaymentStatus status)
    {
        var updateRequest = new UpdatePaymentRequestDto
        {
            Id = id,
            GovPayPaymentId = paymentId,
            UpdatedByUserId = request.UserId.Value,
            UpdatedByOrganisationId = request.OrganisationId.Value,
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

    private void ValidateObject(object obj)
    {
        var context = new ValidationContext(obj, serviceProvider: null, items: null);
        Validator.ValidateObject(obj, context, validateAllProperties: true);
    }
}
