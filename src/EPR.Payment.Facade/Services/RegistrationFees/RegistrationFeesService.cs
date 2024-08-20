using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.Interfaces;

namespace EPR.Payment.Facade.Services.RegistrationFees
{
    public class RegistrationFeesService : IRegistrationFeesService
    {
        private readonly IHttpRegistrationFeesService _httpRegistrationFeesService;

        public RegistrationFeesService(IHttpRegistrationFeesService httpRegistrationFeesService)
        {
            _httpRegistrationFeesService = httpRegistrationFeesService ?? throw new ArgumentNullException(nameof(httpRegistrationFeesService));
        }

        public async Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationFeeRequestDto request)
        {
            return await _httpRegistrationFeesService.CalculateProducerFeesAsync(request);
        }
    }
}
