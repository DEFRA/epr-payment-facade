using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;

namespace EPR.Payment.Facade.Services.RegistrationFees.Interfaces
{
    public interface ISubmissionPeriodsService
    {
        Task<IReadOnlyList<SubmissionPeriodResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    }
}
