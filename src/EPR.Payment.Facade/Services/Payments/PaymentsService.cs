using AutoMapper;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Mappers;
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

        var externalPaymentId = await InsertPaymentAsync(request, cancellationToken);

        var govPayRequest = CreateGovPayRequest(request, externalPaymentId);

        var govPayResponse = await InitiateGovPayPaymentAsync(govPayRequest, cancellationToken);

        await UpdatePaymentStatusAsync(externalPaymentId, request, govPayResponse.PaymentId!, cancellationToken);

        return CreatePaymentResponse(govPayResponse);
    }

    public async Task<CompletePaymentResponseDto> CompletePaymentAsync(Guid externalPaymentId, CancellationToken cancellationToken)
    {
        if (externalPaymentId == Guid.Empty)
        {
            throw new ArgumentException("ExternalPaymentId cannot be empty", nameof(externalPaymentId));
        }

        var paymentDetails = await GetPaymentDetailsAsync(externalPaymentId, cancellationToken);

        if (string.IsNullOrEmpty(paymentDetails.GovPayPaymentId))
        {
            throw new Exception(ExceptionMessages.PaymentStatusNotFound);
        }

        var paymentStatusResponse = await GetPaymentStatusResponseAsync(paymentDetails.GovPayPaymentId, cancellationToken);

        var status = PaymentStatusMapper.GetPaymentStatus(
            paymentStatusResponse.State?.Status ?? throw new Exception(ExceptionMessages.PaymentStatusNotFound),
            paymentStatusResponse.State?.Code
        );

        var updateRequest = CreateUpdatePaymentRequest(paymentDetails, paymentStatusResponse, status);

        await UpdatePaymentAsync(updateRequest, externalPaymentId, cancellationToken);

        return CreateCompletePaymentResponse(paymentDetails, paymentStatusResponse, status);
    }

    private async Task<PaymentDetailsDto> GetPaymentDetailsAsync(Guid externalPaymentId, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpPaymentsService.GetPaymentDetailsAsync(externalPaymentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessages.ErrorGettingPaymentDetails);
            throw new Exception(ExceptionMessages.ErrorRetrievingPaymentDetails, ex);
        }
    }

    private async Task<PaymentStatusResponseDto> GetPaymentStatusResponseAsync(string govPayPaymentId, CancellationToken cancellationToken)
    {
        try
        {
            var paymentStatusResponse = await _httpGovPayService.GetPaymentStatusAsync(govPayPaymentId, cancellationToken);
            if (paymentStatusResponse?.State == null || paymentStatusResponse.State.Status == null)
            {
                throw new Exception(ExceptionMessages.PaymentStatusNotFound);
            }
            return paymentStatusResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessages.ErrorRetrievingPaymentStatus);
            throw new Exception(ExceptionMessages.ErrorRetrievingPaymentStatus, ex);
        }
    }

    private UpdatePaymentRequestDto CreateUpdatePaymentRequest(PaymentDetailsDto paymentDetails, PaymentStatusResponseDto paymentStatusResponse, PaymentStatus status)
    {
        var updateRequest = _mapper.Map<UpdatePaymentRequestDto>(paymentDetails);
        updateRequest.GovPayPaymentId = paymentDetails.GovPayPaymentId;
        updateRequest.Status = status;
        updateRequest.Reference = paymentStatusResponse.Reference;
        updateRequest.ErrorCode = paymentStatusResponse?.State?.Code;
        updateRequest.ErrorMessage = paymentStatusResponse?.State?.Message;

        return updateRequest;
    }

    private async Task UpdatePaymentAsync(UpdatePaymentRequestDto updateRequest, Guid externalPaymentId, CancellationToken cancellationToken)
    {
        try
        {
            await _httpPaymentsService.UpdatePaymentAsync(externalPaymentId, updateRequest, cancellationToken);
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


    private CompletePaymentResponseDto CreateCompletePaymentResponse(PaymentDetailsDto paymentDetails, PaymentStatusResponseDto paymentStatusResponse, PaymentStatus status)
    {
        return new CompletePaymentResponseDto
        {
            Status = status,
            Message = paymentStatusResponse?.State?.Message,
            Reference = paymentStatusResponse?.Reference,
            UserId = paymentDetails.UpdatedByUserId,
            OrganisationId = paymentDetails.UpdatedByOrganisationId,
            Regulator = paymentDetails.Regulator,
            Amount = paymentDetails.Amount,
            Email = paymentStatusResponse?.Email
        };
    }

    private GovPayRequestDto CreateGovPayRequest(PaymentRequestDto request, Guid externalPaymentId)
    {
        var returnUrl = _paymentServiceOptions.ReturnUrl;
        if (string.IsNullOrEmpty(returnUrl))
        {
            throw new InvalidOperationException(ExceptionMessages.ReturnUrlNotConfigured);
        }
        returnUrl = $"{returnUrl}?id={externalPaymentId}";

        var description = _paymentServiceOptions.Description ?? throw new InvalidOperationException(ExceptionMessages.DescriptionNotConfigured);

        var govPayRequest = _mapper.Map<GovPayRequestDto>(request);
        govPayRequest.return_url = returnUrl;
        govPayRequest.Description = description;

        return govPayRequest;
    }


    private async Task<GovPayResponseDto> InitiateGovPayPaymentAsync(GovPayRequestDto govPayRequest, CancellationToken cancellationToken)
    {
        var govPayResponse = await _httpGovPayService.InitiatePaymentAsync(govPayRequest, cancellationToken);

        if (string.IsNullOrEmpty(govPayResponse.PaymentId))
        {
            throw new InvalidOperationException(ExceptionMessages.GovPayResponseInvalid);
        }

        return govPayResponse;
    }

    private async Task UpdatePaymentStatusAsync(Guid externalPaymentId, PaymentRequestDto request, string paymentId, CancellationToken cancellationToken)
    {
        var updateRequest = _mapper.Map<UpdatePaymentRequestDto>(request);
        updateRequest.ExternalPaymentId = externalPaymentId;
        updateRequest.GovPayPaymentId = paymentId;
        updateRequest.Status = PaymentStatus.InProgress;

        try
        {
            await _httpPaymentsService.UpdatePaymentAsync(externalPaymentId, updateRequest, cancellationToken);
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

    private PaymentResponseDto CreatePaymentResponse(GovPayResponseDto govPayResponse)
    {
        return new PaymentResponseDto
        {
            NextUrl = govPayResponse.Links?.NextUrl?.Href
        };
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

    private void ValidateObject(object? obj)
    {
        var context = new ValidationContext(obj!, serviceProvider: null, items: null);
        Validator.ValidateObject(obj!, context, validateAllProperties: true);
    }
}
