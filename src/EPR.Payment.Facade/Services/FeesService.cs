using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services.Interfaces;

namespace EPR.Payment.Facade.Services
{
    public class FeesService : IFeesService
    {
        private readonly IHttpFeesService _httpFeeService;

        public FeesService(IHttpFeesService httpFeeService)
        {
            _httpFeeService = httpFeeService ?? throw new ArgumentNullException(nameof(httpFeeService));
       }

        public async Task<GetFeesResponseDto> GetFeeAsync(bool isLarge, string regulator)
        {
            if (string.IsNullOrEmpty(regulator))
            {
                throw new ArgumentException("Regulator cannot be null or empty.", nameof(regulator));
            }

            return await _httpFeeService.GetFeeAsync(isLarge, regulator);
        }
    }
}
