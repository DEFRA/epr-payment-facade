using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;

namespace EPR.Payment.Facade.Services.Interfaces
{
    public interface IFeesService
    {
        Task<GetFeesResponseDto> GetFeeAsync(bool isLarge, string regulator);
    }
}