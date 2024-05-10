﻿using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services.Interfaces;

namespace EPR.Payment.Facade.Services
{
    public class PaymentServiceHealthService : IPaymentServiceHealthService
    {
        protected readonly IHttpPaymentServiceHealthCheckService _httpPaymentServiceHealthCheckService;

        public PaymentServiceHealthService(
            IHttpPaymentServiceHealthCheckService httpPaymentServiceHealthCheckService)
        {
            _httpPaymentServiceHealthCheckService = httpPaymentServiceHealthCheckService ?? throw new ArgumentNullException(nameof(httpPaymentServiceHealthCheckService));
        }

        public async Task<HttpResponseMessage> GetHealth(CancellationToken cancellationToken)
        {
            return await _httpPaymentServiceHealthCheckService.GetHealth(cancellationToken);
        }
    }
}