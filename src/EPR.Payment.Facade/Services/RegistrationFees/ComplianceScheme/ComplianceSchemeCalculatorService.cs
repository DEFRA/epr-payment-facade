using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme.Interfaces;
using FluentValidation;

namespace EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme
{
    public class ComplianceSchemeCalculatorService : IComplianceSchemeCalculatorService
    {
        private readonly IHttpComplianceSchemeFeesService _httpComplianceSchemeFeesService;
        private readonly IHttpComplianceSchemeFeesServiceV2 _httpComplianceSchemeFeesServiceV2;
        private readonly ILogger<ComplianceSchemeCalculatorService> _logger;

        public ComplianceSchemeCalculatorService(
            IHttpComplianceSchemeFeesService httpComplianceSchemeFeesService,
            IHttpComplianceSchemeFeesServiceV2 httpComplianceSchemeFeesServiceV2,
            ILogger<ComplianceSchemeCalculatorService> logger)
        {
            _httpComplianceSchemeFeesService = httpComplianceSchemeFeesService ?? throw new ArgumentNullException(nameof(httpComplianceSchemeFeesService));
            _httpComplianceSchemeFeesServiceV2 = httpComplianceSchemeFeesServiceV2 ?? throw new ArgumentNullException(nameof(httpComplianceSchemeFeesServiceV2));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ComplianceSchemeFeesResponseDto> CalculateFeesAsync(ComplianceSchemeFeesRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _httpComplianceSchemeFeesService.CalculateFeesAsync(request, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateFeesAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingComplianceSchemeFees);
                throw new ServiceException(ExceptionMessages.ErrorCalculatingComplianceSchemeFees, ex);
            }
        }

        public async Task<ComplianceSchemeFeesResponseDto> CalculateFeesAsync(ComplianceSchemeFeesRequestV2Dto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _httpComplianceSchemeFeesServiceV2.CalculateFeesAsync(request, cancellationToken);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateFeesAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingComplianceSchemeFees);
                throw new ServiceException(ExceptionMessages.ErrorCalculatingComplianceSchemeFees, ex);
            }
        }
    }
}