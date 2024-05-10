using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.HealthCheck;
using EPR.Payment.Facade.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace EPR.Payment.Facade.UnitTests.HealthCheck
{
    [TestClass]
    public class PaymentsFacadeHealthCheckTests : HealthChecksTestsBase
    {
        [TestMethod, AutoMoqData]
        public async Task CheckHealthAsync_ValidQueryResult_ReturnsHealthyStatus(
        [Frozen] Mock<IPaymentServiceHealthService> paymentServiceHealthService,
        HealthCheckContext healthCheckContext,
        PaymentsFacadeHealthCheck paymentsFacadeHealthCheck)
        {
            paymentServiceHealthService.Setup(x => x.GetHealth(It.IsAny<CancellationToken>())).ReturnsAsync(ResponseMessageOk);

            var actual = await paymentsFacadeHealthCheck.CheckHealthAsync(healthCheckContext, CancellationToken.None);

            actual.Status.Should().Be(HealthStatus.Healthy);
        }

        [TestMethod, AutoMoqData]
        public async Task CheckHealthAsync_NotValidQueryResult_ReturnsUnHealthyStatus(
            [Frozen] Mock<IPaymentServiceHealthService> paymentServiceHealthService,
            HealthCheckContext healthCheckContext,
            PaymentsFacadeHealthCheck paymentsFacadeHealthCheck)
        {
            paymentServiceHealthService.Setup(x => x.GetHealth(It.IsAny<CancellationToken>())).ReturnsAsync(ResponseMessageBadRequest);

            var actual = await paymentsFacadeHealthCheck.CheckHealthAsync(healthCheckContext, CancellationToken.None);

            actual.Status.Should().Be(HealthStatus.Unhealthy);
        }
    }
}