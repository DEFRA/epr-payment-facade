using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.Interfaces;

namespace EPR.Payment.Facade.Services.RegistrationFees
{
    public class SubmissionPeriodsService : ISubmissionPeriodsService
    {
        private readonly IHttpSubmissionPeriodsService _httpService;
        private readonly ILogger<SubmissionPeriodsService> _logger;

        public SubmissionPeriodsService(
            IHttpSubmissionPeriodsService httpService,
            ILogger<SubmissionPeriodsService> logger)
        {
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<SubmissionPeriodResponseDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _httpService.GetAllAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorRetrievingSubmissionPeriods);
                throw new ServiceException(ExceptionMessages.ErrorRetrievingSubmissionPeriods, ex);
            }
        }
    }
}
