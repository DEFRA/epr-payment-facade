using EPR.Payment.Facade.Services;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EPR.Payment.Facade.HealthCheck
{
    public class PaymentsFacadeHealthCheck : IHealthCheck
    {
        public const string HealthCheckResultDescription = "Payments Facade Health Check";

        private readonly IFeesService _feesService;
        private readonly IPaymentsService _paymentsService;

        public PaymentsFacadeHealthCheck(IFeesService feesService, IPaymentsService paymentsService)
        {
            _feesService = feesService ?? throw new ArgumentNullException(nameof(feesService));
            _paymentsService = paymentsService ?? throw new ArgumentNullException(nameof(paymentsService));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            //TODO - PS : Add checks here when more details known
            throw new NotImplementedException();
        }
    }
}
