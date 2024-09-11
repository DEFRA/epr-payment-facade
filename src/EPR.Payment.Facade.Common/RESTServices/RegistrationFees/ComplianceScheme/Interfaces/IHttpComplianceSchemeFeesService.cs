using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces
{
    public interface IHttpComplianceSchemeFeesService
    {
        Task<ComplianceSchemeBaseFeeResponse> GetComplianceSchemeBaseFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default);
    }
}