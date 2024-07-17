using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpPaymentsService : BaseHttpService, IHttpPaymentsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;

        public HttpPaymentsService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceBaseUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceEndPointNameMissing))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClientName = config.Value.HttpClientName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceHttpClientNameMissing);
        }

        public async Task<Guid> InsertPaymentAsync(InsertPaymentRequestDto paymentStatusInsertRequest, CancellationToken cancellationToken)
        {
            var url = UrlConstants.PaymentsInsert;
            try
            {
                var response = await Post<Guid>(url, paymentStatusInsertRequest, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionMessages.ErrorInsertingPayment, ex);
            }
        }

        public async Task UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto paymentStatusUpdateRequest, CancellationToken cancellationToken)
        {
            var url = UrlConstants.PaymentsUpdate.Replace("{paymentId}", id.ToString());
            try
            {
                await Put(url, paymentStatusUpdateRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionMessages.ErrorUpdatingPayment, ex);
            }
        }
    }
}
