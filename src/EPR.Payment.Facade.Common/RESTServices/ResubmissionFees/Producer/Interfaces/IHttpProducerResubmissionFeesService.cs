using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces
{
    public interface IHttpProducerResubmissionFeesService
    {
        Task<ProducerResubmissionFeeResponseDto> GetResubmissionFeeAsync(ProducerResubmissionFeeRequestDto request, CancellationToken cancellationToken = default);
    }
}