using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;

namespace EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme.Interfaces
{
    public interface IComplianceSchemeFeesService
    {
        Task<ComplianceSchemeBaseFeeResponse> GetComplianceSchemeBaseFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default);
    }
}