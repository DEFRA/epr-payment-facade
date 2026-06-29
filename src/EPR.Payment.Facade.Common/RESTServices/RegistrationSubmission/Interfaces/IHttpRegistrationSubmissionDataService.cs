using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission.Interfaces
{
    public interface IHttpRegistrationSubmissionDataService
    {
        Task<IReadOnlyList<RegistrationFeeCalculationDetailsDto>?> GetFeeCalculationDetailsAsync(Guid submissionId, CancellationToken cancellationToken = default);
    }
}
