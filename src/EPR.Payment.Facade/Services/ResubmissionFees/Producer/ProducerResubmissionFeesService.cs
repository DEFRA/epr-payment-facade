using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using EPR.Payment.Facade.Services.ResubmissionFees.Producer.Interfaces;
using FluentValidation;

namespace EPR.Payment.Facade.Services.ResubmissionFees.Producer
{
    public class ProducerResubmissionFeesService : IProducerResubmissionFeesService
    {
        private readonly IHttpProducerResubmissionFeesService _httpProducerResubmissionFeesService;
        private readonly IHttpProducerResubmissionFeesServiceV2 _httpProducerResubmissionFeesServiceV2;
        private readonly ILogger<ProducerResubmissionFeesService> _logger;

        public ProducerResubmissionFeesService(IHttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            IHttpProducerResubmissionFeesServiceV2 httpProducerResubmissionFeesServiceV2,
            ILogger<ProducerResubmissionFeesService> logger)
        {
            _httpProducerResubmissionFeesService = httpProducerResubmissionFeesService ?? throw new ArgumentNullException(nameof(httpProducerResubmissionFeesService));
            _httpProducerResubmissionFeesServiceV2 = httpProducerResubmissionFeesServiceV2 ?? throw new ArgumentNullException(nameof(httpProducerResubmissionFeesServiceV2));
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
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(GetResubmissionFeeAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingFees);
                throw;
            }
        }

        public async Task<ProducerResubmissionFeeResponseDto> GetResubmissionFeeAsync(
            ProducerResubmissionFeeRequestV2Dto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorResubmissionFees);

            try
            {
                return await _httpProducerResubmissionFeesServiceV2.GetResubmissionFeeAsync(request, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(GetResubmissionFeeAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingFees);
                throw;
            }
        }
    }
}