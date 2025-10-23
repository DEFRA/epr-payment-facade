using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme.Interfaces;
using FluentValidation;

namespace EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme
{
    public class ComplianceSchemeResubmissionFeesService : IComplianceSchemeResubmissionFeesService
    {
        private readonly IHttpComplianceSchemeResubmissionFeesService _httpComplianceSchemeResubmissionFeesService;
        private readonly IHttpComplianceSchemeResubmissionFeesServiceV2 _httpComplianceSchemeResubmissionFeesServiceV2;
        private readonly ILogger<ComplianceSchemeResubmissionFeesService> _logger;

        public ComplianceSchemeResubmissionFeesService(
            IHttpComplianceSchemeResubmissionFeesService httpComplianceSchemeResubmissionFeesService,
            IHttpComplianceSchemeResubmissionFeesServiceV2 httpComplianceSchemeResubmissionFeesServiceV2,
            ILogger<ComplianceSchemeResubmissionFeesService> logger)
        {
            _httpComplianceSchemeResubmissionFeesService = httpComplianceSchemeResubmissionFeesService ?? throw new ArgumentNullException(nameof(httpComplianceSchemeResubmissionFeesService));
            _httpComplianceSchemeResubmissionFeesServiceV2 = httpComplianceSchemeResubmissionFeesServiceV2 ?? throw new ArgumentNullException(nameof(httpComplianceSchemeResubmissionFeesServiceV2));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ComplianceSchemeResubmissionFeeResponse> CalculateResubmissionFeeAsync(ComplianceSchemeResubmissionFeeRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorResubmissionFees);

            return CalculateComplianceSchemeResubmissionFeeInternalAsync(request, cancellationToken);
        }

        public Task<ComplianceSchemeResubmissionFeeResponse> CalculateResubmissionFeeAsync(ComplianceSchemeResubmissionFeeRequestV2Dto request, CancellationToken cancellationToken = default)
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
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateComplianceSchemeResubmissionFeeInternalAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorResubmissionFees);
                throw new ServiceException(ExceptionMessages.ErrorResubmissionFees, ex);
            }
        }

        private async Task<ComplianceSchemeResubmissionFeeResponse> CalculateComplianceSchemeResubmissionFeeInternalAsync(ComplianceSchemeResubmissionFeeRequestV2Dto request, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpComplianceSchemeResubmissionFeesServiceV2.CalculateResubmissionFeeAsync(request, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateComplianceSchemeResubmissionFeeInternalAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorResubmissionFees);
                throw new ServiceException(ExceptionMessages.ErrorResubmissionFees, ex);
            }
        }
    }
}