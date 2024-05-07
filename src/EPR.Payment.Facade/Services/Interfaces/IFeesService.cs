using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;

namespace EPR.Payment.Facade.Services.Interfaces
{
    public interface IFeesService
    {
        Task<GetFeesResponseDto> GetFee(bool isLarge, string regulator);
    }
}