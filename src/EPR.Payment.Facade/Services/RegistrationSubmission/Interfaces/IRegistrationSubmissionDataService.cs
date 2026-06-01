using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;

namespace EPR.Payment.Facade.Services.RegistrationSubmission.Interfaces
{
    public interface IRegistrationSubmissionDataService
    {
        Task<Guid> CreateAsync(CreateRegistrationSubmissionDataRequest request, CancellationToken cancellationToken = default);
    }
}
