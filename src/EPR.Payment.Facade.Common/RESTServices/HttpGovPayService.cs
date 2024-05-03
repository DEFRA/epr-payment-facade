using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices
{
    public class HttpGovPayService : BaseHttpService, IHttpGovPayService
    {
        private readonly string _bearerToken;

        public HttpGovPayService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
            : base(httpContextAccessor, httpClientFactory, configuration["GovPay:BaseUrl"], configuration["GovPay:EndPointName"])
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _bearerToken = configuration["GovPay:BearerToken"];
        }

        public async Task<PaymentResponseDto> InitiatePayment(PaymentRequestDto paymentRequestDto)
        {
            SetBearerToken(_bearerToken); // Set the bearer token

            var url = "payments";
            try
            {
                return await Post<PaymentResponseDto>(url, paymentRequestDto);
            }
            catch (Exception ex)            {
          
                throw new Exception("Error occurred while initiating payment.", ex);
            }
        }

        public async Task<PaymentStatusResponseDto> GetPaymentStatus(string paymentId)
        {
            SetBearerToken(_bearerToken); // Set the bearer token

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
