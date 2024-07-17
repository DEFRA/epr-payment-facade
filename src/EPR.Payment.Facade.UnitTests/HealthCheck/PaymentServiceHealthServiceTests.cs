using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments;
using FluentAssertions;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.Payments
{
    [TestClass]
    public class PaymentServiceHealthServiceTests
    {
        private Mock<IHttpPaymentServiceHealthCheckService> _httpPaymentServiceHealthCheckServiceMock = null!;
        private PaymentServiceHealthService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _httpPaymentServiceHealthCheckServiceMock = new Mock<IHttpPaymentServiceHealthCheckService>();
            _service = new PaymentServiceHealthService(_httpPaymentServiceHealthCheckServiceMock.Object);
        }

        [TestMethod]
        public void Constructor_WithNullHttpPaymentServiceHealthCheckService_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new PaymentServiceHealthService(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'httpPaymentServiceHealthCheckService')");
        }

        [TestMethod]
        public async Task GetHealthAsync_CallsHttpPaymentServiceHealthCheckService()
        {
            // Arrange
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            _httpPaymentServiceHealthCheckServiceMock.Setup(service => service.GetHealthAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetHealthAsync(CancellationToken.None);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().Be(response);
                _httpPaymentServiceHealthCheckServiceMock.Verify(service => service.GetHealthAsync(It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
