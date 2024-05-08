using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.RESTServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace EPR.Payment.Facade.IntegrationTests
{
    [TestClass]
    public class HttpGovPayServiceIntegrationTests
    {
        private static IConfiguration _configuration;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Load configuration from appsettings.json or any other configuration source
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        [TestMethod]
        public async Task InitiatePayment_Success_PaymentInitiated()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            var service = new HttpGovPayService(httpContextAccessor, httpClientFactory, _configuration);

            var paymentRequestDto = new PaymentRequestDto
            {
                // Provide necessary data for payment request
                Amount = 100,
                Reference = "123456",
                return_url = "https://example.com/return",
                Description = "Test payment"
            };

            // Act & Assert
            await service.Invoking(async x => await x.InitiatePayment(paymentRequestDto))
                .Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task GetPaymentStatus_Success_PaymentStatusRetrieved()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            var service = new HttpGovPayService(httpContextAccessor, httpClientFactory, _configuration);

            var paymentId = "no7kr7it1vjbsvb7r402qqrv86"; // Provide an actual payment ID

            // Act & Assert
            await service.Invoking(async x => await x.GetPaymentStatus(paymentId))
                .Should().NotThrowAsync();
        }
    }
}