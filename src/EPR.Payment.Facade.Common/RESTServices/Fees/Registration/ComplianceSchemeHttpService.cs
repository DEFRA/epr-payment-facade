using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Services.Fees.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EPR.Payment.Facade.Services.Fees
{
    public class ComplianceSchemeHttpService : BaseHttpService, IHttpFeeService
    {
        public ComplianceSchemeHttpService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            string baseUrl,
            string endPointName)
            : base(httpContextAccessor, httpClientFactory, baseUrl, endPointName)
        {
        }

        public async Task<FeeResponse> CalculateFeeAsync(RegistrationFeeRequest request)
        {
            var url = BuildUrlWithQueryString(request);
            return await Get<FeeResponse>(url, false);
        }
    }
}
