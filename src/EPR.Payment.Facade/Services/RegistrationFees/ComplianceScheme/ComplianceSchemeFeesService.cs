using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme.Interfaces;

namespace EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme
{
    public class ComplianceSchemeFeesService : IComplianceSchemeFeesService
    {
        private readonly IHttpComplianceSchemeFeesService _httpComplianceSchemeFeesService;
        private readonly ILogger<ComplianceSchemeFeesService> _logger;

        public ComplianceSchemeFeesService(
            IHttpComplianceSchemeFeesService httpComplianceSchemeFeesService,
            ILogger<ComplianceSchemeFeesService> logger)
        {
            _httpComplianceSchemeFeesService = httpComplianceSchemeFeesService ?? throw new ArgumentNullException(nameof(httpComplianceSchemeFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ComplianceSchemeBaseFeeResponse> GetComplianceSchemeBaseFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                _logger.LogError("Request object is null in {Method}", nameof(GetComplianceSchemeBaseFeeAsync));
                throw new ArgumentNullException(nameof(request), ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee);
            }

            try
            {
                var response = await _httpComplianceSchemeFeesService.GetComplianceSchemeBaseFeeAsync(request, cancellationToken);

                if (response == null || response.BaseFee == 0)
                {
                    _logger.LogError("Invalid base fee returned for regulator {Regulator}", request.Regulator);
                    throw new ServiceException(string.Format(ExceptionMessages.InvalidRegulatorForComplianceScheme, request.Regulator));
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.UnexpectedErrorRetrievingComplianceSchemeBaseFee);
                throw new ServiceException(ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee, ex);
            }
        }
    }
}