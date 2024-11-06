using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using EPR.Payment.Facade.Services.ResubmissionFees.Producer.Interfaces;

namespace EPR.Payment.Facade.Services.ResubmissionFees.Producer
{
    public class ProducerResubmissionFeesService : IProducerResubmissionFeesService
    {
        private readonly IHttpProducerResubmissionFeesService _httpProducerResubmissionFeesService;
        private readonly ILogger<ProducerResubmissionFeesService> _logger;

        public ProducerResubmissionFeesService(IHttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            ILogger<ProducerResubmissionFeesService> logger)
        {
            _httpProducerResubmissionFeesService = httpProducerResubmissionFeesService ?? throw new ArgumentNullException(nameof(httpProducerResubmissionFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProducerResubmissionFeeResponseDto> GetResubmissionFeeAsync(
            ProducerResubmissionFeeRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorResubmissionFees);

            try
            {
                return await _httpProducerResubmissionFeesService.GetResubmissionFeeAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingFees);
                throw;
            }
        }
    }
}