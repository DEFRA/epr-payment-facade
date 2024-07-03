using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpPaymentsService : BaseHttpService, IHttpPaymentsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;

        public HttpPaymentsService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), "PaymentService BaseUrl configuration is missing"),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), "PaymentService EndPointName configuration is missing"))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClientName = config.Value.HttpClientName ?? throw new ArgumentNullException(nameof(config), "PaymentService HttpClientName configuration is missing");
        }

        public async Task<Guid> InsertPaymentAsync(InsertPaymentRequestDto paymentStatusInsertRequest)
        {
            var url = "payments/status";
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
