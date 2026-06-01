using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission.Interfaces
{
    public interface IHttpRegistrationSubmissionDataService
    {
        Task<Guid> CreateAsync(CreateRegistrationSubmissionDataRequest request, CancellationToken cancellationToken = default);
    }
}
