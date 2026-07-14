using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission.Interfaces;
using EPR.Payment.Facade.Services.RegistrationSubmission.Interfaces;

namespace EPR.Payment.Facade.Services.RegistrationSubmission
{
    public class RegistrationSubmissionDataService : IRegistrationSubmissionDataService
    {
        private readonly IHttpRegistrationSubmissionDataService _httpService;
        private readonly ILogger<RegistrationSubmissionDataService> _logger;

        public RegistrationSubmissionDataService(
            IHttpRegistrationSubmissionDataService httpService,
            ILogger<RegistrationSubmissionDataService> logger)
        {
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<RegistrationFeeCalculationDetailsDto>?> GetFeeCalculationDetailsAsync(Guid submissionId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _httpService.GetFeeCalculationDetailsAsync(submissionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorRetrievingRegistrationFeeCalculationDetails);
                throw new ServiceException(ExceptionMessages.ErrorRetrievingRegistrationFeeCalculationDetails, ex);
            }
        }
    }
}
