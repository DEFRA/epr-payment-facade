using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.Producer.Interfaces;
using FluentValidation;

namespace EPR.Payment.Facade.Services.RegistrationFees.Producer
{
    public class ProducerFeesService : IProducerFeesService
    {
        private readonly IHttpProducerFeesService _httpProducerFeesService;
        private readonly IHttpProducerFeesV3Service _httpProducerFeeServiceV2;
        private readonly ILogger<ProducerFeesService> _logger;

        public ProducerFeesService(IHttpProducerFeesService httpProducerFeesService,
            IHttpProducerFeesV3Service httpProducerFeesServiceV2,
            ILogger<ProducerFeesService> logger)
        {
            _httpProducerFeesService = httpProducerFeesService ?? throw new ArgumentNullException(nameof(httpProducerFeesService));
            _httpProducerFeeServiceV2 = httpProducerFeesServiceV2 ?? throw new ArgumentNullException(nameof(httpProducerFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProducerFeesResponseDto> CalculateProducerFeesAsync(ProducerFeesRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorCalculatingProducerFees);

            return await CalculateProducerFeesInternalAsync(request);
        }

        public async Task<ProducerFeesResponseDto> CalculateProducerFeesAsync(ProducerFeesRequestV3Dto request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorCalculatingProducerFees);

            return await CalculateProducerFeesInternalV3Async(request);
        }

        private async Task<ProducerFeesResponseDto> CalculateProducerFeesInternalAsync(ProducerFeesRequestDto request)
        {
            try
            {
                var response = await _httpProducerFeesService.CalculateProducerFeesAsync(request);
                return response;
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateProducerFeesInternalAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingProducerFees);
                throw new ServiceException(ExceptionMessages.ErrorCalculatingProducerFees, ex);
            }
        }

        private async Task<ProducerFeesResponseDto> CalculateProducerFeesInternalV3Async(ProducerFeesRequestV3Dto request)
        {
            try
            {
                var response = await _httpProducerFeeServiceV2.CalculateProducerFeesAsync(request);
                return response;
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateProducerFeesInternalAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingProducerFees);
                throw new ServiceException(ExceptionMessages.ErrorCalculatingProducerFees, ex);
            }
        }
    }
}