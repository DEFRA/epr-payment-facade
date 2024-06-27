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
            // Here you can check if request.PayBaseFeeAlone is true and handle it accordingly.
            if (request.PayBaseFeeAlone)
            {
                // Implement logic to handle paying base fee alone
                // This may involve calling a specific endpoint or modifying request handling
                throw new NotImplementedException("Paying base fee alone is not implemented yet.");
            }

            // Implement the logic to call the external service for producer fees
            throw new NotImplementedException("CalculateProducerFeesAsync is not implemented yet.");
        }

        public async Task<RegistrationFeeResponseDto> CalculateComplianceSchemeFeesAsync(ComplianceSchemeRegistrationRequestDto request)
        {
            // Here you can check if request.PayBaseFeeAlone is true and handle it accordingly.
            if (request.PayBaseFeeAlone)
            {
                // Implement logic to handle paying base fee alone
                // This may involve calling a specific endpoint or modifying request handling
                throw new NotImplementedException("Paying base fee alone is not implemented yet.");
            }

            // Implement the logic to call the external service for compliance scheme fees
            throw new NotImplementedException("CalculateComplianceSchemeFeesAsync is not implemented yet.");
        }
    }
}
