using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpPaymentsService : BaseHttpService, IHttpPaymentsService
    {
        private readonly string? _bearerToken;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;

        public HttpPaymentsService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
            : base(httpContextAccessor, httpClientFactory,
                configuration?["PaymentService:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "BaseUrl configuration is missing"),
                configuration?["PaymentService:EndPointName"] ?? throw new ArgumentNullException(nameof(configuration), "EndPointName configuration is missing"))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _bearerToken = configuration["PaymentService:BearerToken"] ?? throw new ArgumentNullException(nameof(configuration), "Bearer token configuration is missing");
            _httpClientName = configuration["PaymentService:HttpClientName"] ?? throw new ArgumentNullException(nameof(configuration), "HttpClientName configuration is missing");
        }

        public async Task<Guid> InsertPaymentAsync(InsertPaymentRequestDto paymentStatusInsertRequest)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {
                throw new InvalidOperationException("Bearer token is null. Unable to insert payment.");
            }

            var url = $"payments/status";
            try
            {
                var response = await PostWithResponse<InsertPaymentResponseDto>(url, paymentStatusInsertRequest);
                return response.ExternalPaymentId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while inserting payment status.", ex);
            }
        }

        public async Task UpdatePaymentAsync(Guid externalPaymentId, UpdatePaymentRequestDto paymentStatusUpdateRequest)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {
                throw new InvalidOperationException("Bearer token is null. Unable to update payment.");
            }

            var url = $"payments/{externalPaymentId}/status";
            try
            {
                await Put(url, paymentStatusUpdateRequest);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while updating payment status.", ex);
            }
        }

        private async Task<T> PostWithResponse<T>(string url, object content)
        {
            using var httpClient = _httpClientFactory.CreateClient(_httpClientName);
            var response = await httpClient.PostAsJsonAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
