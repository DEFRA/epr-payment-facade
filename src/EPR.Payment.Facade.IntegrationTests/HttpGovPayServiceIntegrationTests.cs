using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using FluentAssertions;
using FluentAssertions.Execution;
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
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        [TestMethod]
        public async Task InitiatePayment_Success_PaymentResponseValid()
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

            var service = new HttpGovPayService(httpContextAccessor!, httpClientFactory!, options!);

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
            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                response.PaymentId.Should().NotBeNullOrEmpty();
                response.State?.Status.Should().NotBeNullOrEmpty();
                response.Amount.Should().Be(paymentRequestDto.Amount);
                response.Reference.Should().Be(paymentRequestDto.Reference);
                response.Description.Should().Be(paymentRequestDto.Description);
                response.Links?.NextUrl?.Href.Should().NotBeNullOrEmpty();
            }
        }

        [TestMethod]
        public async Task InitiatePayment_BearerTokenNull_ThrowsInvalidOperationException()
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

            var service = new TestHttpGovPayService(httpContextAccessor!, httpClientFactory!, options!, null);

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
                .WithMessage("Error occurred while initiating payment.");
        }

        [TestMethod]
        public async Task InitiatePayment_PostThrowsException_ThrowsException()
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

            var service = new TestHttpGovPayService(httpContextAccessor!, httpClientFactory!, options!, options!.Value.BearerToken);

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

            // Intentionally set an incorrect URL to force an exception
            service.SetBaseUrl("https://invalid-url.com");

            // Act & Assert
            await service.Invoking(async x => await x.InitiatePaymentAsync(paymentRequestDto, cancellationToken))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Error occurred while initiating payment.");
        }

        [TestMethod]
        public async Task GetPaymentStatus_BearerTokenNull_ThrowsInvalidOperationException()
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

            var service = new TestHttpGovPayService(httpContextAccessor!, httpClientFactory!, options!, null);

            var paymentId = "test-payment-id";
            var cancellationToken = new CancellationToken();

            // Act & Assert
            await service.Invoking(async x => await x.GetPaymentStatusAsync(paymentId, cancellationToken))
                .Should().ThrowAsync<ServiceException>()
                .WithMessage("Error occurred while retrieving payment status.");
        }

        [TestMethod]
        public async Task GetPaymentStatus_PostThrowsException_ThrowsException()
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

            var service = new TestHttpGovPayService(httpContextAccessor!, httpClientFactory!, options!, options!.Value.BearerToken);

            var paymentId = "test-payment-id";
            var cancellationToken = new CancellationToken();

            // Intentionally set an incorrect URL to force an exception
            service.SetBaseUrl("https://invalid-url.com");

            // Act & Assert
            await service.Invoking(async x => await x.GetPaymentStatusAsync(paymentId, cancellationToken))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Error occurred while retrieving payment status.");
        }
    }

    public class TestHttpGovPayService : HttpGovPayService
    {
        public TestHttpGovPayService(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<Service> config,
            string? bearerToken)
            : base(httpContextAccessor, httpClientFactory, config)
        {
            // Use reflection to set the private _bearerToken field to the provided value
            var field = typeof(HttpGovPayService).GetField("_bearerToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(this, bearerToken);
        }

        // Provide a method to set the Base URL for testing
        public void SetBaseUrl(string baseUrl)
        {
            var field = typeof(BaseHttpService).GetField("_baseUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(this, baseUrl);
        }
    }
}
