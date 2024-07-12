using AutoMapper;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
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
    private readonly IMapper _mapper;

    public PaymentsService(
        IHttpGovPayService httpGovPayService,
        IHttpPaymentsService httpPaymentsService,
        ILogger<PaymentsService> logger,
        IOptions<PaymentServiceOptions> paymentServiceOptions,
        IMapper mapper)
    {
        _httpGovPayService = httpGovPayService ?? throw new ArgumentNullException(nameof(httpGovPayService));
        _httpPaymentsService = httpPaymentsService ?? throw new ArgumentNullException(nameof(httpPaymentsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _paymentServiceOptions = paymentServiceOptions.Value ?? throw new ArgumentNullException(nameof(paymentServiceOptions));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        ValidateObject(request);

        // Use the static values from configuration
        var returnUrl = _paymentServiceOptions.ReturnUrl ?? throw new InvalidOperationException(ExceptionMessages.ReturnUrlNotConfigured);
        var description = _paymentServiceOptions.Description ?? throw new InvalidOperationException(ExceptionMessages.DescriptionNotConfigured);

        // Map PaymentRequestDto to GovPayPaymentRequestDto using AutoMapper
        var govPayRequest = _mapper.Map<GovPayPaymentRequestDto>(request);
        govPayRequest.return_url = returnUrl;
        govPayRequest.Description = description;

        var externalPaymentId = await InsertPaymentAsync(request!, cancellationToken);

        var govPayResponse = await _httpGovPayService.InitiatePaymentAsync(govPayRequest, cancellationToken);

        if (string.IsNullOrEmpty(govPayResponse.PaymentId))
        {
            throw new InvalidOperationException(ExceptionMessages.GovPayResponseInvalid);
        }

        await UpdatePaymentAsync(externalPaymentId, request, govPayResponse.PaymentId, PaymentStatus.InProgress, cancellationToken);

        return new PaymentResponseDto
        {
            NextUrl = govPayResponse.Links?.NextUrl?.Href
        };
    }

    public async Task CompletePaymentAsync(string? govPayPaymentId, CompletePaymentRequestDto completeRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(govPayPaymentId))
            throw new ArgumentException(ExceptionMessages.GovPayPaymentIdNull, nameof(govPayPaymentId));

        var paymentStatusResponse = await _httpGovPayService.GetPaymentStatusAsync(govPayPaymentId, cancellationToken);
        if (paymentStatusResponse == null || paymentStatusResponse.State == null)
            throw new Exception(ExceptionMessages.PaymentStatusNotFound);

        PaymentStatus? status = paymentStatusResponse.State.Status switch
        {
            "success" => PaymentStatus.Success,
            "failed" => PaymentStatus.Failed,
            "error" => PaymentStatus.Error,
            _ => null
        };

        if (status == null)
            throw new Exception(ExceptionMessages.PaymentStatusNotFound);

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
            await _httpPaymentsService.UpdatePaymentAsync(completeRequest.Id, updateRequest, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, LogMessages.ValidationErrorUpdatingPayment);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UnexpectedErrorUpdatingPayment);
            throw new Exception(ExceptionMessages.UnexpectedErrorUpdatingPayment, ex);
        }
    }

    private async Task<Guid> InsertPaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        var insertRequest = _mapper.Map<InsertPaymentRequestDto>(request);
        insertRequest.ReasonForPayment = _paymentServiceOptions.Description;
        insertRequest.Status = PaymentStatus.Initiated;

        try
        {
            return await _httpPaymentsService.InsertPaymentAsync(insertRequest, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, LogMessages.ValidationErrorInsertingPayment);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UnexpectedErrorInsertingPayment);
            throw new Exception(ExceptionMessages.UnexpectedErrorInsertingPayment, ex);
        }
    }

    private async Task UpdatePaymentAsync(Guid id, PaymentRequestDto request, string paymentId, PaymentStatus status, CancellationToken cancellationToken)
    {
        var updateRequest = _mapper.Map<UpdatePaymentRequestDto>(request);
        updateRequest.Id = id;
        updateRequest.GovPayPaymentId = paymentId;
        updateRequest.Status = status;

        try
        {
            await _httpPaymentsService.UpdatePaymentAsync(id, updateRequest, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, LogMessages.ValidationErrorUpdatingPayment);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UnexpectedErrorUpdatingPayment);
            throw new Exception(ExceptionMessages.UnexpectedErrorUpdatingPayment, ex);
        }
    }

    private void ValidateObject(object? obj)
    {
        var context = new ValidationContext(obj!, serviceProvider: null, items: null);
        Validator.ValidateObject(obj!, context, validateAllProperties: true);
    }
}
