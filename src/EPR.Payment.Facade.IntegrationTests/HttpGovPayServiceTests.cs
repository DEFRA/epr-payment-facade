using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
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
                .Configure<Service>(options => _configuration.GetSection("GovPayService").Bind(options))
                .BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var options = serviceProvider.GetService<IOptions<Service>>();

            var service = new HttpGovPayService(httpContextAccessor, httpClientFactory, options);

            var paymentRequestDto = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "123456",
                return_url = "https://example.com/return",
                ReasonForPayment = "Test payment",
                UserId = "userId",
                OrganisationId = "organisationId",
                Regulator = "regulator",
                Description = "Payment description"
            };

            // Act & Assert
            await service.Invoking(async x => await x.InitiatePaymentAsync(paymentRequestDto))
                .Should().NotThrowAsync();
        }


        [TestMethod]
        public async Task GetPaymentStatus_Success_PaymentStatusRetrieved()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .Configure<Service>(options => _configuration.GetSection("GovPayService").Bind(options))
                .BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var options = serviceProvider.GetService<IOptions<Service>>();

            var service = new HttpGovPayService(httpContextAccessor, httpClientFactory, options);

            var paymentId = "no7kr7it1vjbsvb7r402qqrv86"; // Provide an actual payment ID

            // Act & Assert
            await service.Invoking(async x => await x.GetPaymentStatusAsync(paymentId))
                .Should().NotThrowAsync();
        }
    }
}
