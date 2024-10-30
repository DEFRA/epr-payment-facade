using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using FluentValidation;

namespace EPR.Payment.Facade.Services.Payments
{
    public class OfflinePaymentsService : IOfflinePaymentsService
    {
        private readonly IHttpOfflinePaymentsService _httpOfflinePaymentsService;
        private readonly IValidator<OfflinePaymentRequestDto> _offlinePaymentRequestDtoValidator;

        public OfflinePaymentsService(
            IHttpOfflinePaymentsService httpOfflinePaymentsService,
            IValidator<OfflinePaymentRequestDto> offlinePaymentRequestDtoValidator)
        { 
            _httpOfflinePaymentsService = httpOfflinePaymentsService ?? throw new ArgumentNullException(nameof(httpOfflinePaymentsService));
            _offlinePaymentRequestDtoValidator = offlinePaymentRequestDtoValidator ?? throw new ArgumentNullException(nameof(offlinePaymentRequestDtoValidator));
        }

        public async Task OfflinePaymentAsync(OfflinePaymentRequestDto request, CancellationToken cancellationToken = default)
        {
            var validatorResult = await _offlinePaymentRequestDtoValidator.ValidateAsync(request, cancellationToken);

            if (!validatorResult.IsValid)
            {
                throw new ValidationException(validatorResult.Errors.Aggregate("", (current, error) => current + $"\n{error.PropertyName} : {error.ErrorMessage}"));
            }
                await _httpOfflinePaymentsService.InsertOfflinePaymentAsync(request, cancellationToken);
        }
    }
}