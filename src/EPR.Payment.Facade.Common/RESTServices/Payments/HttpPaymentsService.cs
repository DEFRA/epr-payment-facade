using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Exceptions;
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
                throw new ServiceException(ExceptionMessages.ErrorInsertingPayment, ex);
            }
        }

        public async Task UpdatePaymentAsync(Guid externalPaymentId, UpdatePaymentRequestDto paymentStatusUpdateRequest, CancellationToken cancellationToken)
        {
            var url = UrlConstants.PaymentsUpdate.Replace("{externalPaymentId}", externalPaymentId.ToString());
            try
            {
                await Put(url, paymentStatusUpdateRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorUpdatingPayment, ex);
            }
        }

        public async Task<PaymentDetailsDto> GetPaymentDetailsAsync(Guid externalPaymentId, CancellationToken cancellationToken)
        {
            var url = UrlConstants.GetPaymentDetails.Replace("{externalPaymentId}", externalPaymentId.ToString());
            try
            {
                var response = await Get<PaymentDetailsDto>(url, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorGettingPaymentDetails, ex);
            }
        }
    }
}
