using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;

namespace EPR.Payment.Facade.Services.RegistrationSubmission.Interfaces
{
    public interface IRegistrationSubmissionDataService
    {
        Task<IReadOnlyList<RegistrationFeeCalculationDetailsDto>?> GetFeeCalculationDetailsAsync(Guid submissionId, CancellationToken cancellationToken = default);
    }
}
