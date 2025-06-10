using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.V2Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpOnlinePaymentsV2Service : BaseHttpService, IHttpOnlinePaymentsV2Service
    {
        public HttpOnlinePaymentsV2Service(
           HttpClient httpClient,
           IHttpContextAccessor httpContextAccessor,
           IOptionsMonitor<Service> configMonitor)
           : base(httpClient,
                  httpContextAccessor,
                  configMonitor.Get("OnlineV2PaymentService").Url
                      ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OnlinePaymentServiceBaseUrlMissing),
                  configMonitor.Get("OnlineV2PaymentService").EndPointName
                      ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OnlinePaymentServiceEndPointNameMissing))
        {
            var config = configMonitor.Get("OnlineV2PaymentService");
        }

        public async Task<Guid> InsertOnlinePaymentAsync(InsertOnlinePaymentRequestV2Dto onlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.OnlinePaymentsInsert;
            try
            {
                var response = await Post<Guid>(url, onlinePaymentStatusInsertRequest, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorInsertingOnlinePayment, ex);
            }
        }
    }
}
