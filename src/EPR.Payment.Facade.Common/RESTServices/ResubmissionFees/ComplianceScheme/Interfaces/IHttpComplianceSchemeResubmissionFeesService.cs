using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces
{
    public interface IHttpComplianceSchemeResubmissionFeesService
    {
        Task<ComplianceSchemeResubmissionFeeResult> CalculateResubmissionFeeAsync(ComplianceSchemeResubmissionFeeRequestDto request, CancellationToken cancellationToken = default);
    }
}