using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces
{
    public interface IHttpSubmissionPeriodsService
    {
        Task<IReadOnlyList<SubmissionPeriodResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    }
}
