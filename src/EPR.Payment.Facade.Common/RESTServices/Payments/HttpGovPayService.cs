using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpGovPayService : BaseHttpService, IHttpGovPayService
    {
        private readonly string? _bearerToken;
        private readonly AsyncRetryPolicy<GovPayResponseDto> _retryPolicy;
        private readonly AsyncRetryPolicy<PaymentStatusResponseDto?> _statusRetryPolicy;

        public HttpGovPayService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config)
            : base(httpContextAccessor, httpClientFactory,
                config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceBaseUrlMissing),
                config.Value.EndPointName ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.PaymentServiceEndPointNameMissing))
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _bearerToken = config.Value.BearerToken ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.BearerTokenNull);

            // Define the retry policy for InitiatePaymentAsync
            int retries = config.Value.Retries ?? 1;
            _retryPolicy = Policy<GovPayResponseDto>
                .Handle<HttpRequestException>()
                .OrResult(result => string.IsNullOrEmpty(result.PaymentId)) // Retry if the PaymentId is empty
                .WaitAndRetryAsync(retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (result, timeSpan, retryCount, context) =>
                    {
                        // Log or handle the retry attempt here
                        // Use _logger if available
                    });
        }

        public async Task<GovPayResponseDto> InitiatePaymentAsync(GovPayRequestDto paymentRequestDto, CancellationToken cancellationToken)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {
                throw new InvalidOperationException(ExceptionMessages.BearerTokenNull);
            }

            var url = UrlConstants.GovPayInitiatePayment;
            try
            {
                // Use the retry policy when calling the Post method
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await Post<GovPayResponseDto>(url, paymentRequestDto, cancellationToken);
                });
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorInitiatingPayment, ex);
            }
        }

        public async Task<PaymentStatusResponseDto?> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken)
        {
            if (_bearerToken != null)
            {
                SetBearerToken(_bearerToken); // Set the bearer token
            }
            else
            {
                throw new InvalidOperationException(ExceptionMessages.BearerTokenNull);
            }

            var url = UrlConstants.GovPayGetPaymentStatus.Replace("{paymentId}", paymentId);
            try
            {
                    return await Get<PaymentStatusResponseDto?>(url, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorRetrievingPaymentStatus, ex);
            }
        }
    }
}
