﻿using EPR.Payment.Facade.Common.Configuration;
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
        public HttpPaymentsService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceBaseUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceEndPointNameMissing))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<Guid> InsertPaymentAsync(InsertPaymentRequestDto paymentStatusInsertRequest, CancellationToken cancellationToken = default)
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

        public async Task UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto paymentStatusUpdateRequest, CancellationToken cancellationToken = default)
        {
            var url = UrlConstants.PaymentsUpdate.Replace("{externalPaymentId}", id.ToString());
            try
            {
                await Put(url, paymentStatusUpdateRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorUpdatingPayment, ex);
            }
        }

        public async Task<PaymentDetailsDto> GetPaymentDetailsAsync(Guid externalPaymentId, CancellationToken cancellationToken = default)
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
