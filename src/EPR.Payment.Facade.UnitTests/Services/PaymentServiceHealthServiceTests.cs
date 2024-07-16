using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments;
using EPR.Payment.Facade.UnitTests.HealthCheck;
using FluentAssertions;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services
{
    [TestClass]
    public class PaymentServiceHealthServiceTests : HealthChecksTestsBase
    {
        [TestMethod, Common.UnitTests.TestHelpers.AutoMoqData]
        public async Task GetHealthAsync_ValidQueryResult_ReturnsHttpStatusOK(
            [Frozen] Mock<IHttpPaymentServiceHealthCheckService> httpPaymentServiceHealthCheckService,
            PaymentServiceHealthService paymentServiceHealthService)
        {
            //Arrange
            httpPaymentServiceHealthCheckService.Setup(x => x.GetHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ResponseMessageOk);

            //Act
            var actual = await paymentServiceHealthService.GetHealthAsync(CancellationToken.None);

            //Assert
            actual.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [TestMethod, Common.UnitTests.TestHelpers.AutoMoqData]
        public async Task GetHealthAsync_ValidQueryResult_ReturnsHttpStatusBasRequest(
            [Frozen] Mock<IHttpPaymentServiceHealthCheckService> httpPaymentServiceHealthCheckService,
            PaymentServiceHealthService paymentServiceHealthService)
        {
            //Arrange
            httpPaymentServiceHealthCheckService.Setup(x => x.GetHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ResponseMessageBadRequest);

            //Act
            var actual = await paymentServiceHealthService.GetHealthAsync(CancellationToken.None);

            //Assert
            actual.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
