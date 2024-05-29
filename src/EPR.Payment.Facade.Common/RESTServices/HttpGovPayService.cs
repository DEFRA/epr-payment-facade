using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices
{
    public class HttpGovPayService : BaseHttpService, IHttpGovPayService
    {
        private readonly string? _bearerToken;

        public HttpGovPayService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
            : base(httpContextAccessor, httpClientFactory,
                configuration?["GovPay:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "BaseUrl configuration is missing"),
                configuration?["GovPay:EndPointName"] ?? throw new ArgumentNullException(nameof(configuration), "EndPointName configuration is missing"))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _bearerToken = configuration["GovPay:BearerToken"] ?? throw new ArgumentNullException(nameof(configuration), "Bearer token configuration is missing");
        }

        public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto paymentRequestDto)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {        
                throw new InvalidOperationException("Bearer token is null. Unable to initiate payment.");
            }

            var url = "payments";
            try
            {
                return await Post<PaymentResponseDto>(url, paymentRequestDto);
            }
            catch (Exception ex)            {
          
                throw new Exception("Error occurred while initiating payment.", ex);
            }
        }

        public async Task<PaymentStatusResponseDto> GetPaymentStatusAsync(string paymentId)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {            
                throw new InvalidOperationException("Bearer token is null. Unable to retrieve payment status.");
            }

            var url = $"payments/{paymentId}";
            try
            {
                return await Get<PaymentStatusResponseDto>(url);
            }
            catch (Exception ex)
            {         
                throw new Exception("Error occurred while retrieving payment status.", ex);
            }
        }
    }
}
