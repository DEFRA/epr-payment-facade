using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;

namespace EPR.Payment.Facade.Services.ResubmissionFees.Producer
{
    public class ProducerResubmissionFeesService
    {
        private readonly IHttpProducerResubmissionFeesService _httpProducerResubmissionFeesService;

        public ProducerResubmissionFeesService(IHttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            ILogger<ProducerResubmissionFeesService> logger)
        {
            _httpProducerResubmissionFeesService = httpProducerResubmissionFeesService ?? throw new ArgumentNullException(nameof(httpProducerResubmissionFeesService));
        }

        public async Task<decimal?> GetResubmissionFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorResubmissionFees);

            return await _httpProducerResubmissionFeesService.GetResubmissionFeeAsync(request, cancellationToken);
        }
    }
}