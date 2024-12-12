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
    public class HttpOnlinePaymentsService : BaseHttpService, IHttpOnlinePaymentsService
    {
        public HttpOnlinePaymentsService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<Service> configMonitor)
            : base(httpClient,
                   httpContextAccessor,
                   configMonitor.Get("PaymentService").Url
                       ?? throw new ArgumentNullException(nameof(configMonitor), ExceptionMessages.OnlinePaymentServiceBaseUrlMissing))
        {
            var config = configMonitor.Get("PaymentService");
            Console.WriteLine($"HttpOnlinePaymentsService initialized with BaseUrl: {config.Url}");
        }

        public async Task<Guid> InsertOnlinePaymentAsync(InsertOnlinePaymentRequestDto onlinePaymentStatusInsertRequest, CancellationToken cancellationToken = default)
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

        public async Task UpdateOnlinePaymentAsync(Guid id, UpdateOnlinePaymentRequestDto onlinePaymentStatusUpdateRequest, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.OnlinePaymentsUpdate.Replace("{externalPaymentId}", id.ToString());
            try
            {
                await Put(url, onlinePaymentStatusUpdateRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorUpdatingOnlinePayment, ex);
            }
        }

        public async Task<OnlinePaymentDetailsDto> GetOnlinePaymentDetailsAsync(Guid externalPaymentId, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.GetOnlinePaymentDetails.Replace("{externalPaymentId}", externalPaymentId.ToString());
            try
            {
                var response = await Get<OnlinePaymentDetailsDto>(url, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorGettingOnlinePaymentDetails, ex);
            }
        }
    }
}
