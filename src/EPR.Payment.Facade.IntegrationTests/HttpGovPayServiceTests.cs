using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.RESTServices;


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

            // Act
            try
            {
                var paymentResponse = await service.InitiatePayment(paymentRequestDto);

                // Assert
                Assert.IsNotNull(paymentResponse);
                // Add more assertions based on the expected behavior
            }
            catch (Exception ex)
            {
                // Handle or log any exceptions
                Assert.Fail($"Exception occurred: {ex.Message}");
            }
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

            // Act
            try
            {
                var paymentStatusResponse = await service.GetPaymentStatus(paymentId);

                // Assert
                Assert.IsNotNull(paymentStatusResponse);
                // Add more assertions based on the expected behavior
            }
            catch (Exception ex)
            {
                // Handle or log any exceptions
                Assert.Fail($"Exception occurred: {ex.Message}");
            }
        }
    }
}