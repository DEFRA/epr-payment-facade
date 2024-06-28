using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices
{
    public class HttpFeesService : BaseHttpService, IHttpFeesService
    {
        public HttpFeesService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            string baseUrl)
            : base(httpContextAccessor, httpClientFactory, baseUrl, "fees")
        {
        }

        public async Task<RegistrationFeeResponseDto> CalculateProducerFeesAsync(ProducerRegistrationRequestDto request)
        {
            return await Post<RegistrationFeeResponseDto>("producer", request);
        }

        public async Task<RegistrationFeeResponseDto> CalculateComplianceSchemeFeesAsync(ComplianceSchemeRegistrationRequestDto request)
        {
            return await Post<RegistrationFeeResponseDto>("compliancescheme", request);
        }
    }
}
