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
        private Mock<IHttpOfflinePaymentsServiceV2> _httpOfflinePaymentsServiceV2Mock = null!;
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
            _httpOfflinePaymentsServiceV2Mock = _fixture.Freeze<Mock<IHttpOfflinePaymentsServiceV2>>();

            _service = new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                _httpOfflinePaymentsServiceV2Mock.Object);
        }

        [TestMethod]
        public void Constructor_WhenAllDependenciesAreNotNull_ShouldCreateInstance()
        {
            // Act
            _service = new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                _httpOfflinePaymentsServiceV2Mock.Object);

            // Assert
            _service.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WhenHttpOfflinePaymentsServiceIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new OfflinePaymentsService(
                null!, _httpOfflinePaymentsServiceV2Mock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpOfflinePaymentsService");
        }

        [TestMethod]
        public void Constructor_WhenHttpOfflinePaymentsServiceV2IsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpOfflinePaymentsServiceV2");
        }

        [TestMethod]
        public async Task InitiateOfflinePayment_ValidRequest_ReturnsResponse()
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

        [TestMethod]
        public async Task InitiateOfflinePaymentV2_ValidRequest_ReturnsResponse()
        {
            // Arrange

            _httpOfflinePaymentsServiceV2Mock.Setup(s => s.InsertOfflinePaymentAsync(It.IsAny<OfflinePaymentRequestV2Dto>(), It.IsAny<CancellationToken>()));

            var request = _fixture.Build<OfflinePaymentRequestV2Dto>().With(d => d.UserId, Guid.NewGuid()).Create();

            // Act
            await _service.OfflinePaymentAsync(request, new CancellationToken());

            // Assert
            using (new AssertionScope())
            {
                _httpOfflinePaymentsServiceV2Mock.Verify(s => s.InsertOfflinePaymentAsync(It.IsAny<OfflinePaymentRequestV2Dto>(), It.IsAny<CancellationToken>()), Times.Once);
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