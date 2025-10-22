using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces
{
    public interface IHttpComplianceSchemeResubmissionFeesServiceV2
    {
        Task<ComplianceSchemeResubmissionFeeResponse> CalculateResubmissionFeeAsync(ComplianceSchemeResubmissionFeeRequestV2Dto request, CancellationToken cancellationToken = default);
    }
}