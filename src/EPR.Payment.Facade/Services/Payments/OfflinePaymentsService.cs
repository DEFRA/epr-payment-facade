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
    public class OfflinePaymentsService : IOfflinePaymentsService
    {
        private readonly IHttpOfflinePaymentsService _httpOfflinePaymentsService;
        private readonly ILogger<OfflinePaymentsService> _logger;
        private readonly IMapper _mapper;
    private readonly IValidator<OfflinePaymentRequestDto> _offlinePaymentRequestDtoValidator;

        public OfflinePaymentsService(
            IHttpOfflinePaymentsService httpOfflinePaymentsService,
            ILogger<OfflinePaymentsService> logger,
            IMapper mapper,
            IValidator<OfflinePaymentRequestDto> offlinePaymentRequestDtoValidator)
        { 
            _httpOfflinePaymentsService = httpOfflinePaymentsService ?? throw new ArgumentNullException(nameof(httpOfflinePaymentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _offlinePaymentRequestDtoValidator = offlinePaymentRequestDtoValidator ?? throw new ArgumentNullException(nameof(offlinePaymentRequestDtoValidator));
        }

        public async Task<OfflinePaymentResponseDto> OfflinePaymentAsync(OfflinePaymentRequestDto request, CancellationToken cancellationToken = default)
        {
        var validatorResult = await _offlinePaymentRequestDtoValidator.ValidateAsync(request, cancellationToken);

        if (!validatorResult.IsValid)
        {
            throw new ValidationException(validatorResult.Errors.Aggregate("", (current, error) => current + $"\n{error.PropertyName} : {error.ErrorMessage}"));
        }
            var externalPaymentId = await InsertOfflinePaymentAsync(request, cancellationToken);

            return new OfflinePaymentResponseDto { PaymentId = externalPaymentId } ;
        }

        private async Task<Guid> InsertOfflinePaymentAsync(OfflinePaymentRequestDto request, CancellationToken cancellationToken)
        {
            var insertRequest = _mapper.Map<InsertOfflinePaymentRequestDto>(request);

            try
            {
                return await _httpOfflinePaymentsService.InsertOfflinePaymentAsync(insertRequest, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorInsertingOfflinePayment);
                throw new ServiceException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.UnexpectedErrorInsertingOfflinePayment);
                throw new ServiceException(ExceptionMessages.UnexpectedErrorInsertingOfflinePayment, ex);
            }
        }
    }
}