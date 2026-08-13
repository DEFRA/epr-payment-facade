using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;

namespace EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme.Interfaces
{
    public interface IComplianceSchemeCalculatorService
    {
        Task<ComplianceSchemeFeesResponseDto> CalculateFeesAsync(ComplianceSchemeFeesRequestDto request, CancellationToken cancellationToken = default);

        Task<ComplianceSchemeFeesResponseDto?> GetFeesBySubmissionAsync(Guid submissionId, CancellationToken cancellationToken = default);
    }
}