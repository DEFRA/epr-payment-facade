using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme.Interfaces;

namespace EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme
{
    public class ComplianceSchemeResubmissionFeesService : IComplianceSchemeResubmissionFeesService
    {
        private readonly IHttpComplianceSchemeResubmissionFeesService _httpComplianceSchemeResubmissionFeesService;
        private readonly ILogger<ComplianceSchemeResubmissionFeesService> _logger;

        public ComplianceSchemeResubmissionFeesService(
            IHttpComplianceSchemeResubmissionFeesService httpComplianceSchemeResubmissionFeesService,
            ILogger<ComplianceSchemeResubmissionFeesService> logger)
        {
            _httpComplianceSchemeResubmissionFeesService = httpComplianceSchemeResubmissionFeesService ?? throw new ArgumentNullException(nameof(httpComplianceSchemeResubmissionFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ComplianceSchemeResubmissionFeeResponse> CalculateResubmissionFeeAsync(ComplianceSchemeResubmissionFeeRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorResubmissionFees);

            return CalculateComplianceSchemeResubmissionFeeInternalAsync(request, cancellationToken);
        }

        private async Task<ComplianceSchemeResubmissionFeeResponse> CalculateComplianceSchemeResubmissionFeeInternalAsync(ComplianceSchemeResubmissionFeeRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpComplianceSchemeResubmissionFeesService.CalculateResubmissionFeeAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorResubmissionFees);
                throw new ServiceException(ExceptionMessages.ErrorResubmissionFees, ex);
            }
        }
    }
}