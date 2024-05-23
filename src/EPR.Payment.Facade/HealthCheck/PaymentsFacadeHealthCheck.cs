using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EPR.Payment.Facade.HealthCheck
{
    public class PaymentsFacadeHealthCheck : IHealthCheck
    {
        public const string HealthCheckResultDescription = "Payments Facade Health Check";
        private readonly IPaymentServiceHealthService _paymentServiceHealthService;


        public PaymentsFacadeHealthCheck(IPaymentServiceHealthService paymentServiceHealthService)
        {
            _paymentServiceHealthService = paymentServiceHealthService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var response = await _paymentServiceHealthService.GetHealthAsync(cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy(HealthCheckResultDescription)
                : HealthCheckResult.Unhealthy(HealthCheckResultDescription);
        }
    }
}
