using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;

namespace EPR.Payment.Facade.Services.ResubmissionFees.Producer.Interfaces
{
    public interface IProducerResubmissionFeesService
    {
        Task<decimal?> GetResubmissionFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default);
    }
}
