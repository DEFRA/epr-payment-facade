using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;

namespace EPR.Payment.Facade.Services.Payments
{
    public class OfflinePaymentsService : IOfflinePaymentsService
    {
        private readonly IHttpOfflinePaymentsService _httpOfflinePaymentsService;

        public OfflinePaymentsService(
            IHttpOfflinePaymentsService httpOfflinePaymentsService)
        {
            _httpOfflinePaymentsService = httpOfflinePaymentsService ?? throw new ArgumentNullException(nameof(httpOfflinePaymentsService));
        }

        public async Task OfflinePaymentAsync(OfflinePaymentRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorResubmissionFees);

            await _httpOfflinePaymentsService.InsertOfflinePaymentAsync(request, cancellationToken);
        }

        public async Task OfflinePaymentAsync(OfflinePaymentRequestV2Dto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorResubmissionFees);

            await _httpOfflinePaymentsService.InsertOfflinePaymentAsync(request, cancellationToken);
        }
    }
}