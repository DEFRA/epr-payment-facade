using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission.Interfaces
{
    public interface IHttpRegistrationSubmissionDataService
    {
        Task<Guid> CreateAsync(CreateRegistrationSubmissionDataRequest request, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RegistrationFeeCalculationDetailsDto>?> GetFeeCalculationDetailsAsync(Guid submissionId, CancellationToken cancellationToken = default);
    }
}
