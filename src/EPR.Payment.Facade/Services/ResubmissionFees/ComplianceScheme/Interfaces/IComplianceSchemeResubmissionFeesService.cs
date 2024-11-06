using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;

namespace EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme.Interfaces
{
    public interface IComplianceSchemeResubmissionFeesService
    {
        Task<ComplianceSchemeResubmissionFeeResponse> CalculateResubmissionFeeAsync(ComplianceSchemeResubmissionFeeRequestDto request, CancellationToken cancellationToken = default);
    }
}