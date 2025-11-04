using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces
{
    public interface IHttpProducerResubmissionFeesServiceV2
    {
        Task<ProducerResubmissionFeeResponseDto> GetResubmissionFeeAsync(ProducerResubmissionFeeRequestV2Dto request, CancellationToken cancellationToken = default);
    }
}