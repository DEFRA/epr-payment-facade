using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;

namespace EPR.Payment.Facade.Services.Payments
{
    public class OfflinePaymentsService : IOfflinePaymentsService
    {
        private readonly IHttpOfflinePaymentsService _httpOfflinePaymentsService;
        private readonly IHttpOfflinePaymentsServiceV2 _httpOfflinePaymentsServiceV2;

        public OfflinePaymentsService(
            IHttpOfflinePaymentsService httpOfflinePaymentsService, IHttpOfflinePaymentsServiceV2 httpOfflinePaymentsServiceV2)
        {
            _httpOfflinePaymentsService = httpOfflinePaymentsService ?? throw new ArgumentNullException(nameof(httpOfflinePaymentsService));
            _httpOfflinePaymentsServiceV2 = httpOfflinePaymentsServiceV2 ?? throw new ArgumentNullException(nameof(httpOfflinePaymentsServiceV2));
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

            await _httpOfflinePaymentsServiceV2.InsertOfflinePaymentAsync(request, cancellationToken);
        }
    }
}