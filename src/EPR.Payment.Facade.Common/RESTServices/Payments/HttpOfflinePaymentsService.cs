using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpOfflinePaymentsService : BaseHttpService, IHttpOfflinePaymentsService
    {
        public HttpOfflinePaymentsService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<Service> config)
            : base(httpClient,
                   httpContextAccessor,
                   config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.OfflinePaymentServiceBaseUrlMissing))
        {
        }

        public async Task InsertOfflinePaymentAsync(OfflinePaymentRequestDto offlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.OfflinePaymentsInsert;
            try
            {
                await Post<Guid>(url, offlinePaymentStatusInsertRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorInsertingOfflinePayment, ex);
            }
        }
    }
}
