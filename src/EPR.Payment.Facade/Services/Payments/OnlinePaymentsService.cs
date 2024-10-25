using AutoMapper;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.Mappers;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.Extensions.Options;
using FluentValidation;

namespace EPR.Payment.Facade.Services.Payments
{
    public class OnlinePaymentsService : IOnlinePaymentsService
    {
        private readonly IHttpGovPayService _httpGovPayService;
        private readonly IHttpOnlinePaymentsService _httpOnlinePaymentsService;
        private readonly ILogger<OnlinePaymentsService> _logger;
        private readonly OnlinePaymentServiceOptions _onlinePaymentServiceOptions;
        private readonly IMapper _mapper;
    private readonly IValidator<OnlinePaymentRequestDto> _onlinePaymentRequestDtoValidator;

        public OnlinePaymentsService(
            IHttpGovPayService httpGovPayService,
            IHttpOnlinePaymentsService httpOnlinePaymentsService,
            ILogger<OnlinePaymentsService> logger,
            IOptions<OnlinePaymentServiceOptions> onlinePaymentServiceOptions,
            IMapper mapper,
            IValidator<OnlinePaymentRequestDto> onlinePaymentRequestDtoValidator)
        {
            _httpGovPayService = httpGovPayService ?? throw new ArgumentNullException(nameof(httpGovPayService));
            _httpOnlinePaymentsService = httpOnlinePaymentsService ?? throw new ArgumentNullException(nameof(httpOnlinePaymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _onlinePaymentServiceOptions = onlinePaymentServiceOptions.Value ?? throw new ArgumentNullException(nameof(onlinePaymentServiceOptions));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _onlinePaymentRequestDtoValidator = onlinePaymentRequestDtoValidator ?? throw new ArgumentNullException(nameof(onlinePaymentRequestDtoValidator));
        }

        public async Task<OnlinePaymentResponseDto> InitiateOnlinePaymentAsync(OnlinePaymentRequestDto request, CancellationToken cancellationToken = default)
        {
        var validatorResult = await _onlinePaymentRequestDtoValidator.ValidateAsync(request, cancellationToken);

        if (!validatorResult.IsValid)
        {
            throw new ValidationException(validatorResult.Errors.Aggregate("", (current, error) => current + $"\n{error.PropertyName} : {error.ErrorMessage}"));
        }
            var externalPaymentId = await InsertOnlinePaymentAsync(request, cancellationToken);

            var govPayRequest = CreateGovPayRequest(request, externalPaymentId);

            var govPayResponse = await InitiateGovPayPaymentAsync(govPayRequest, cancellationToken);

            await UpdateOnlinePaymentStatusAsync(externalPaymentId, request, govPayResponse.PaymentId!, cancellationToken);

            return CreateOnlinePaymentResponse(govPayResponse);
        }

        public async Task<CompleteOnlinePaymentResponseDto> CompleteOnlinePaymentAsync(Guid externalPaymentId, CancellationToken cancellationToken = default)
        {
            if (externalPaymentId == Guid.Empty)
            {
                throw new ArgumentException("ExternalPaymentId cannot be empty", nameof(externalPaymentId));
            }

            var onlinePaymentDetails = await GetOnlinePaymentDetailsAsync(externalPaymentId, cancellationToken);

            if (string.IsNullOrEmpty(onlinePaymentDetails.GovPayPaymentId))
            {
                throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
            }

            var paymentStatusResponse = await GetPaymentStatusResponseAsync(onlinePaymentDetails.GovPayPaymentId, cancellationToken);

            var status = PaymentStatusMapper.GetPaymentStatus(
                paymentStatusResponse.State?.Status ?? throw new ServiceException(ExceptionMessages.PaymentStatusNotFound),
                paymentStatusResponse.State?.Code
            );

            var updateRequest = CreateOnlineUpdatePaymentRequest(onlinePaymentDetails, paymentStatusResponse, status);

            await UpdateOnlinePaymentAsync(updateRequest, externalPaymentId, cancellationToken);

            return CreateCompleteOnlinePaymentResponse(onlinePaymentDetails, paymentStatusResponse, status);
        }

        private async Task<OnlinePaymentDetailsDto> GetOnlinePaymentDetailsAsync(Guid externalPaymentId, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpOnlinePaymentsService.GetOnlinePaymentDetailsAsync(externalPaymentId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorGettingOnlinePaymentDetails);
                throw new ServiceException(ExceptionMessages.ErrorRetrievingOnlinePaymentDetails, ex);
            }
        }

        private async Task<PaymentStatusResponseDto> GetPaymentStatusResponseAsync(string govPayPaymentId, CancellationToken cancellationToken)
        {
            try
            {
                var paymentStatusResponse = await _httpGovPayService.GetPaymentStatusAsync(govPayPaymentId, cancellationToken);
                if (paymentStatusResponse?.State == null || paymentStatusResponse.State.Status == null)
                {
                    throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
                }
                return paymentStatusResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorRetrievingPaymentStatus);
                throw new ServiceException(ExceptionMessages.ErrorRetrievingPaymentStatus, ex);
            }
        }

        private UpdateOnlinePaymentRequestDto CreateOnlineUpdatePaymentRequest(OnlinePaymentDetailsDto onlinePaymentDetails, PaymentStatusResponseDto paymentStatusResponse, PaymentStatus status)
        {
            var updateRequest = _mapper.Map<UpdateOnlinePaymentRequestDto>(onlinePaymentDetails);
            updateRequest.GovPayPaymentId = onlinePaymentDetails.GovPayPaymentId;
            updateRequest.Status = status;
            updateRequest.Reference = paymentStatusResponse.Reference;
            updateRequest.ErrorCode = paymentStatusResponse.State?.Code;
            updateRequest.ErrorMessage = paymentStatusResponse.State?.Message;

            return updateRequest;
        }

        private async Task UpdateOnlinePaymentAsync(UpdateOnlinePaymentRequestDto updateRequest, Guid externalPaymentId, CancellationToken cancellationToken)
        {
            try
            {
                await _httpOnlinePaymentsService.UpdateOnlinePaymentAsync(externalPaymentId, updateRequest, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorUpdatingOnlinePayment);
                throw new ServiceException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.UnexpectedErrorUpdatingOnlinePayment);
                throw new ServiceException(ExceptionMessages.UnexpectedErrorUpdatingOnlinePayment, ex);
            }
        }


        private CompleteOnlinePaymentResponseDto CreateCompleteOnlinePaymentResponse(OnlinePaymentDetailsDto onlinePaymentDetails, PaymentStatusResponseDto paymentStatusResponse, PaymentStatus status)
        {
            return new CompleteOnlinePaymentResponseDto
            {
                Status = status,
                Message = paymentStatusResponse?.State?.Message,
                Reference = paymentStatusResponse?.Reference,
                UserId = onlinePaymentDetails.UpdatedByUserId,
                OrganisationId = onlinePaymentDetails.UpdatedByOrganisationId,
                Regulator = onlinePaymentDetails.Regulator,
                Amount = onlinePaymentDetails.Amount,
                Email = paymentStatusResponse?.Email
            };
        }

        private GovPayRequestDto CreateGovPayRequest(OnlinePaymentRequestDto request, Guid externalPaymentId)
        {
            var returnUrl = _onlinePaymentServiceOptions.ReturnUrl;
            if (string.IsNullOrEmpty(returnUrl))
            {
                throw new InvalidOperationException(ExceptionMessages.ReturnUrlNotConfigured);
            }
            returnUrl = $"{returnUrl}?id={externalPaymentId}";

            var description = _onlinePaymentServiceOptions.Description ?? throw new InvalidOperationException(ExceptionMessages.DescriptionNotConfigured);

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

        private async Task UpdateOnlinePaymentStatusAsync(Guid externalPaymentId, OnlinePaymentRequestDto request, string paymentId, CancellationToken cancellationToken)
        {
            var updateRequest = _mapper.Map<UpdateOnlinePaymentRequestDto>(request);
            updateRequest.ExternalPaymentId = externalPaymentId;
            updateRequest.GovPayPaymentId = paymentId;
            updateRequest.Status = PaymentStatus.InProgress;

            try
            {
                await _httpOnlinePaymentsService.UpdateOnlinePaymentAsync(externalPaymentId, updateRequest, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorUpdatingOnlinePayment);
                throw new ServiceException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.UnexpectedErrorUpdatingOnlinePayment);
                throw new ServiceException(ExceptionMessages.UnexpectedErrorUpdatingOnlinePayment, ex);
            }
        }

        private OnlinePaymentResponseDto CreateOnlinePaymentResponse(GovPayResponseDto govPayResponse)
        {
            return new OnlinePaymentResponseDto
            {
                NextUrl = govPayResponse.Links?.NextUrl?.Href
            };
        }

        private async Task<Guid> InsertOnlinePaymentAsync(OnlinePaymentRequestDto request, CancellationToken cancellationToken)
        {
            var insertRequest = _mapper.Map<InsertOnlinePaymentRequestDto>(request);
            insertRequest.ReasonForPayment = _onlinePaymentServiceOptions.Description;
            insertRequest.Status = PaymentStatus.Initiated;

            try
            {
                return await _httpOnlinePaymentsService.InsertOnlinePaymentAsync(insertRequest, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorInsertingOnlinePayment);
                throw new ServiceException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.UnexpectedErrorInsertingOnlinePayment);
                throw new ServiceException(ExceptionMessages.UnexpectedErrorInsertingOnlinePayment, ex);
            }
        }
    }
}