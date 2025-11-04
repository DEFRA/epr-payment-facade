using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces
{
    public interface IHttpComplianceSchemeFeesServiceV2
    {
        Task<ComplianceSchemeFeesResponseDto> CalculateFeesAsync(ComplianceSchemeFeesRequestV2Dto request, CancellationToken cancellationToken = default);
    }
}