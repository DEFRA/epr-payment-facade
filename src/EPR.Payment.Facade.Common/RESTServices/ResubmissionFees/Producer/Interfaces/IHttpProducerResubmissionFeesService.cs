using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces
{
    public interface IHttpProducerResubmissionFeesService
    {
        Task<decimal?> GetResubmissionFeeAsync(RegulatorDto request, CancellationToken cancellationToken = default);
    }
}
