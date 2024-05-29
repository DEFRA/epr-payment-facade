using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;

namespace EPR.Payment.Facade.Services.Payments
{
    public class PaymentServiceHealthService : IPaymentServiceHealthService
    {
        protected readonly IHttpPaymentServiceHealthCheckService _httpPaymentServiceHealthCheckService;

        public PaymentServiceHealthService(
            IHttpPaymentServiceHealthCheckService httpPaymentServiceHealthCheckService)
        {
            _httpPaymentServiceHealthCheckService = httpPaymentServiceHealthCheckService ?? throw new ArgumentNullException(nameof(httpPaymentServiceHealthCheckService));
        }

        public async Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken)
        {
            return await _httpPaymentServiceHealthCheckService.GetHealthAsync(cancellationToken);
        }
    }
}