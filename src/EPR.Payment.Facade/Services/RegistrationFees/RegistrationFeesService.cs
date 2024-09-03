using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.Interfaces;

namespace EPR.Payment.Facade.Services.RegistrationFees
{
    public class RegistrationFeesService : IRegistrationFeesService
    {
        private readonly IHttpRegistrationFeesService _httpRegistrationFeesService;
        private readonly ILogger<RegistrationFeesService> _logger;

        public RegistrationFeesService(IHttpRegistrationFeesService httpRegistrationFeesService,
            ILogger<RegistrationFeesService> logger)
        {
            _httpRegistrationFeesService = httpRegistrationFeesService ?? throw new ArgumentNullException(nameof(httpRegistrationFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<RegistrationFeesResponseDto> CalculateProducerFeesAsync(ProducerRegistrationFeesRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorCalculatingProducerFees);

            return CalculateProducerFeesInternalAsync(request);
        }
        public async Task<decimal?> GetResubmissionFeeAsync(string regulator, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(regulator))
            {
                throw new ArgumentException(ExceptionMessages.RegulatorCanNotBeNullOrEmpty, nameof(regulator));
            }

            return await _httpRegistrationFeesService.GetResubmissionFeeAsync(regulator, cancellationToken);
        }

        private async Task<RegistrationFeesResponseDto> CalculateProducerFeesInternalAsync(ProducerRegistrationFeesRequestDto request)
        {
            try
            {
                var response = await _httpRegistrationFeesService.CalculateProducerFeesAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingProducerFees);
                throw new ServiceException(ExceptionMessages.ErrorCalculatingProducerFees, ex);
            }
        }
    }
}