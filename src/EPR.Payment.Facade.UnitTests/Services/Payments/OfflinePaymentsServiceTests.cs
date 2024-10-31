using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.Payments;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.Payments
{
    [TestClass]
    public class OfflinePaymentsServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpOfflinePaymentsService> _httpOfflinePaymentsServiceMock = null!;
        private OfflinePaymentsService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            var throwingRecursionBehaviors = _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList();
            foreach (var behavior in throwingRecursionBehaviors)
            {
                _fixture.Behaviors.Remove(behavior);
            }
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _httpOfflinePaymentsServiceMock = _fixture.Freeze<Mock<IHttpOfflinePaymentsService>>();

            _service = new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object);
        }

        [TestMethod]
        public void Constructor_WhenAllDependenciesAreNotNull_ShouldCreateInstance()
        {
            // Act
            var service = new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WhenHttpOfflinePaymentsServiceIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new OfflinePaymentsService(
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpOfflinePaymentsService");
        }

        [TestMethod]
        public async Task InitiateOfflinePayment_ValidRequest_ReturnsRespons()
        {
            // Arrange

            _httpOfflinePaymentsServiceMock.Setup(s => s.InsertOfflinePaymentAsync(It.IsAny<OfflinePaymentRequestDto>(), It.IsAny<CancellationToken>()));

            var request = _fixture.Build<OfflinePaymentRequestDto>().With(d => d.UserId, Guid.NewGuid()).Create();

            // Act
            await _service.OfflinePaymentAsync(request, new CancellationToken());

            // Assert
            using (new AssertionScope())
            {
                _httpOfflinePaymentsServiceMock.Verify(s => s.InsertOfflinePaymentAsync(It.IsAny<OfflinePaymentRequestDto>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task OfflinePayment_NullRequest_ThrowsArgumentNullException(
            OfflinePaymentsService service)
        {
            // Act & Assert
            await service.Invoking(async s => await s.OfflinePaymentAsync(null!, new CancellationToken()))
                .Should().ThrowAsync<ArgumentNullException>();
        }
    }
}