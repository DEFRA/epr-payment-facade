using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;

namespace EPR.Payment.Facade.Services.ResubmissionFees.Producer.Interfaces
{
    public interface IProducerResubmissionFeesService
    {
        Task<ProducerResubmissionFeeResponseDto> GetResubmissionFeeAsync(ProducerResubmissionFeeRequestDto request, CancellationToken cancellationToken = default);
    }
}