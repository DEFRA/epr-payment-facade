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
            IOptionsMonitor<Service> configMonitor)
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("OfflinePaymentService").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OfflinePaymentServiceBaseUrlMissing),
                   configMonitor.Get("OfflinePaymentService").EndPointName
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OfflinePaymentServiceEndPointNameMissing))
        {
            _ = configMonitor.Get("OfflinePaymentService");
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
