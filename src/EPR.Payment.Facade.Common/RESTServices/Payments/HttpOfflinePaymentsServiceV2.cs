using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpOfflinePaymentsServiceV2 : BaseHttpService, IHttpOfflinePaymentsServiceV2
    {
        public HttpOfflinePaymentsServiceV2(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<Service> configMonitor)
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("OfflinePaymentServiceV2").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OfflinePaymentServiceBaseUrlMissing),
                   configMonitor.Get("OfflinePaymentServiceV2").EndPointName
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OfflinePaymentServiceEndPointNameMissing))
        {
            _ = configMonitor.Get("OfflinePaymentServiceV2");
        }       

        public async Task InsertOfflinePaymentAsync(OfflinePaymentRequestV2Dto offlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default)
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
