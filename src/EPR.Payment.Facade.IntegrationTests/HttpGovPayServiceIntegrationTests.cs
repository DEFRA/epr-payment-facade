using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.IntegrationTests
{
    [TestClass]
    public class HttpGovPayServiceIntegrationTests
    {
        private static IConfiguration? _configuration;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Load configuration from appsettings.json or any other configuration source
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private static ServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .Configure<Service>(options => _configuration?.GetSection("GovPayService").Bind(options))
                .BuildServiceProvider();
        }

        private static HttpGovPayService CreateService(ServiceProvider serviceProvider)
        {
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var options = serviceProvider.GetService<IOptions<Service>>();

            return new HttpGovPayService(
                httpClientFactory!.CreateClient(),
                httpContextAccessor!,
                options!);
        }

        [TestMethod]
        public async Task InitiatePayment_Success_PaymentResponseValid()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var service = CreateService(serviceProvider);

            var paymentRequestDto = new GovPayRequestDto
            {
                Amount = 100,
                Reference = "123456",
                return_url = "https://example.com/return",
                Description = "Payment description",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "regulator"
            };

            var cancellationToken = new CancellationToken();

            // Act
            var response = await service.InitiatePaymentAsync(paymentRequestDto, cancellationToken);

            // Assert
            response.Should().NotBeNull();
            response.PaymentId.Should().NotBeNullOrEmpty();
            response.State?.Status.Should().NotBeNullOrEmpty();
            response.Amount.Should().Be(paymentRequestDto.Amount);
            response.Reference.Should().Be(paymentRequestDto.Reference);
            response.Description.Should().Be(paymentRequestDto.Description);
            response.Links?.NextUrl?.Href.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task InitiatePayment_BearerTokenNull_ThrowsServiceException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .Configure<Service>(options => _configuration?.GetSection("GovPayService").Bind(options))
                .BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var options = serviceProvider.GetService<IOptions<Service>>();

            // Pass a null bearer token to simulate the scenario
            var service = new TestHttpGovPayService(
                httpClientFactory!.CreateClient(),
                httpContextAccessor!,
                options!,
                null);

            var paymentRequestDto = new GovPayRequestDto
            {
                Amount = 100,
                Reference = "123456",
                return_url = "https://example.com/return",
                Description = "Payment description",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "regulator"
            };

            var cancellationToken = new CancellationToken();

            // Act & Assert
            await service.Invoking(async x => await x.InitiatePaymentAsync(paymentRequestDto, cancellationToken))
                .Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorInitiatingPayment);
        }

        [TestMethod]
        public async Task InitiatePayment_PostThrowsException_ThrowsServiceException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .Configure<Service>(options => _configuration?.GetSection("GovPayService").Bind(options))
                .BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var options = serviceProvider.GetService<IOptions<Service>>();

            // Initialize the TestHttpGovPayService with a valid bearer token
            var service = new TestHttpGovPayService(
                httpClientFactory!.CreateClient(),
                httpContextAccessor!,
                options!,
                options!.Value.BearerToken);

            var paymentRequestDto = new GovPayRequestDto
            {
                Amount = 100,
                Reference = "123456",
                return_url = "https://example.com/return",
                Description = "Payment description",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "regulator"
            };

            var cancellationToken = new CancellationToken();

            // Intentionally set an invalid Base URL to simulate a network or HTTP error
            service.SetBaseUrl("https://invalid-url.com");

            // Act & Assert
            await service.Invoking(async x => await x.InitiatePaymentAsync(paymentRequestDto, cancellationToken))
                .Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorInitiatingPayment);
        }

        [TestMethod]
        public async Task GetPaymentStatus_BearerTokenNull_ThrowsServiceException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .Configure<Service>(options => _configuration?.GetSection("GovPayService").Bind(options))
                .BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var options = serviceProvider.GetService<IOptions<Service>>();

            // Initialize the TestHttpGovPayService with a null bearer token
            var service = new TestHttpGovPayService(
                httpClientFactory!.CreateClient(),
                httpContextAccessor!,
                options!,
                null); // Explicitly set the bearer token to null

            var paymentId = "test-payment-id";
            var cancellationToken = new CancellationToken();

            // Act & Assert
            await service.Invoking(async x => await x.GetPaymentStatusAsync(paymentId, cancellationToken))
                .Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
        }

        [TestMethod]
        public async Task GetPaymentStatus_PostThrowsException_ThrowsServiceException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .Configure<Service>(options => _configuration?.GetSection("GovPayService").Bind(options))
                .BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var options = serviceProvider.GetService<IOptions<Service>>();

            // Create the TestHttpGovPayService with a valid bearer token
            var service = new TestHttpGovPayService(
                httpClientFactory!.CreateClient(),
                httpContextAccessor!,
                options!,
                options!.Value.BearerToken);

            var paymentId = "test-payment-id";
            var cancellationToken = new CancellationToken();

            // Intentionally set an incorrect URL to simulate an exception
            service.SetBaseUrl("https://invalid-url.com");

            // Act & Assert
            await service.Invoking(async x => await x.GetPaymentStatusAsync(paymentId, cancellationToken))
                .Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
        }

        public class TestHttpGovPayService : HttpGovPayService
        {
            public TestHttpGovPayService(
                HttpClient httpClient,
                IHttpContextAccessor httpContextAccessor,
                IOptions<Service> config,
                string? bearerToken = null)
                : base(httpClient, httpContextAccessor, config)
            {
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    // Set the Authorization header directly on the HttpClient
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                }
            }

            // Provide a method to set the Base URL for testing
            public void SetBaseUrl(string baseUrl)
            {
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
                }

                // Use reflection to update the _baseUrl field in BaseHttpService
                var field = typeof(BaseHttpService)
                    .GetField("_baseUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field == null)
                {
                    throw new InvalidOperationException("The '_baseUrl' field could not be found in BaseHttpService.");
                }
                field.SetValue(this, baseUrl);
            }
        }

    }
}
