using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using AutoMapper;
using EPR.Payment.Common.Mapping;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.Common;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.Payments;
using EPR.Payment.Facade.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.Payments
{
    [TestClass]
    public class OfflinePaymentsServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpOfflinePaymentsService> _httpOfflinePaymentsServiceMock = null!;
        private Mock<ILogger<OfflinePaymentsService>> _loggerMock = null!;
        private Mock<IOptions<OfflinePaymentServiceOptions>> _optionsMock = null!;
        private OfflinePaymentsService _service = null!;
        private IMapper _mapper = null!;
        private Mock<IValidator<OfflinePaymentRequestDto>> _offlinePaymentRequestDtoMock = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _offlinePaymentRequestDtoMock = new Mock<IValidator<OfflinePaymentRequestDto>>();
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            var throwingRecursionBehaviors = _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList();
            foreach (var behavior in throwingRecursionBehaviors)
            {
                _fixture.Behaviors.Remove(behavior);
            }
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _httpOfflinePaymentsServiceMock = _fixture.Freeze<Mock<IHttpOfflinePaymentsService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<OfflinePaymentsService>>>();
            _optionsMock = _fixture.Freeze<Mock<IOptions<OfflinePaymentServiceOptions>>>();

            _optionsMock.Setup(o => o.Value).Returns(new OfflinePaymentServiceOptions
            {
                Description = "Payment description" //TODO: Do I really need Description in OfflinePaymentServiceOptions????
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OfflinePaymentRequestMappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _service = new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _mapper,
                _offlinePaymentRequestDtoMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenAllDependenciesAreNotNull_ShouldCreateInstance(
            [Frozen] OfflinePaymentServiceOptions _offlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpOfflinePaymentsService> _httpOfflinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OfflinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OfflinePaymentRequestDto>> _offlinePaymentRequestDtoValidatorMock)
        {
            // Arrange
            _offlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_offlinePaymentServiceOptions);

            // Act
            var service = new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _offlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _offlinePaymentRequestDtoValidatorMock.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpOfflinePaymentsServiceIsNull_ShouldThrowArgumentNullException(
            [Frozen] OfflinePaymentServiceOptions _offlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock,
            [Frozen] Mock<ILogger<OfflinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OfflinePaymentRequestDto>> _offlinePaymentRequestDtoValidatorMock)
        {
            // Arrange
            _offlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_offlinePaymentServiceOptions);

            // Act
            Action act = () => new OfflinePaymentsService(
                null!,
                _loggerMock.Object,
                _offlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _offlinePaymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpOfflinePaymentsService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException(
            [Frozen] OfflinePaymentServiceOptions _offlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpOfflinePaymentsService> _httpOfflinePaymentsServiceMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OfflinePaymentRequestDto>> _offlinePaymentRequestDtoValidatorMock)
        {
            // Arrange
            _offlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_offlinePaymentServiceOptions);

            // Act
            Action act = () => new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                null!,
                _offlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _offlinePaymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenOfflinePaymentServiceOptionsIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpOfflinePaymentsService> _httpOfflinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OfflinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OfflinePaymentRequestDto>> _offlinePaymentRequestDtoValidatorMock)
        {
            // Arrange
            _offlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns((OfflinePaymentServiceOptions)null!);

            // Act
            Action act = () => new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _offlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _offlinePaymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("offlinePaymentServiceOptions");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenMapperIsNull_ShouldThrowArgumentNullException(
            [Frozen] OfflinePaymentServiceOptions _offlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpOfflinePaymentsService> _httpOfflinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OfflinePaymentsService>> _loggerMock,
            [Frozen] Mock<IValidator<OfflinePaymentRequestDto>> _offlinePaymentRequestDtoValidatorMock)
        {
            // Arrange
            _offlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_offlinePaymentServiceOptions);

            // Act
            Action act = () => new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _offlinePaymentServiceOptionsMock.Object,
                null!,
                _offlinePaymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenOfflinePaymentRequestDtoValidatorIsNull_ShouldThrowArgumentNullException(
            [Frozen] OfflinePaymentServiceOptions _offlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpOfflinePaymentsService> _httpOfflinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OfflinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock)
        {
            // Arrange
            _offlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_offlinePaymentServiceOptions);

            // Act
            Action act = () => new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _offlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("offlinePaymentRequestDtoValidator");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOfflinePayment_ValidRequest_ReturnsResponse(
            GovPayResponseDto expectedResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();

            _httpOfflinePaymentsServiceMock.Setup(s => s.InsertOfflinePaymentAsync(It.IsAny<InsertOfflinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(externalPaymentId);

            var request = _fixture.Build<OfflinePaymentRequestDto>().With(d => d.UserId, new Guid()).Create();
            _offlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            // Act
            var result = await _service.OfflinePaymentAsync(request, new CancellationToken());

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PaymentId.Should().Be(externalPaymentId);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task OfflinePayment_NullRequest_ThrowsArgumentNullException(
            OfflinePaymentsService service)
        {
            // Act & Assert
            await service.Invoking(async s => await s.OfflinePaymentAsync(null!, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task OfflinePayment_MissingFields_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OfflinePaymentRequestDto>().With(d => d.UserId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.UserId), "User ID is required.")
            };

            _offlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.OfflinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task OfflinePayment_DescriptionNotConfigured_ThrowsDescriptionNotConfiguredException(
            OfflinePaymentRequestDto request)
        {
            // Arrange
            _offlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var offlinePaymentServiceOptions = new OfflinePaymentServiceOptions { Description = null };
            _optionsMock.Setup(o => o.Value).Returns(offlinePaymentServiceOptions);

            var service = new OfflinePaymentsService(
                _httpOfflinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _mapper,
                _offlinePaymentRequestDtoMock.Object);

            // Act & Assert
            await service.Invoking(async s => await s.OfflinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.DescriptionNotConfigured);
        }

        [TestMethod]
        public async Task OfflinePayment_MissingUserId_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OfflinePaymentRequestDto>().With(d => d.UserId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.UserId), "User ID is required.")
            };

            _offlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.OfflinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task OfflinePayment_MissingAmount_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OfflinePaymentRequestDto>().With(d => d.Amount, (int?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required.")
            };

            _offlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.OfflinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task OfflinePayment_ValidationExceptionThrown_LogsAndThrows(
            OfflinePaymentRequestDto request)
        {
            // Arrange
            _offlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var validationException = new ValidationException(ExceptionMessages.ErrorInsertingOfflinePayment);
            _httpOfflinePaymentsServiceMock.Setup(s => s.InsertOfflinePaymentAsync(It.IsAny<InsertOfflinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act & Assert
            var exception = await _service.Invoking(async s => await s.OfflinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ServiceException>();

            // Use a flexible matching to ensure the message contains the expected constant message
            using (new AssertionScope())
            {
                exception.Which.Message.Should().Match($"*{ExceptionMessages.ErrorInsertingOfflinePayment}*");
            }

            // Verify log entry
            _loggerMock.VerifyLog(LogLevel.Error, LogMessages.ValidationErrorInsertingOfflinePayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOfflinePayment_UnexpectedExceptionThrown_LogsAndThrows(
            OfflinePaymentRequestDto request)
        {
            // Arrange
            _offlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var unexpectedException = new Exception("Unexpected error");
            _httpOfflinePaymentsServiceMock.Setup(s => s.InsertOfflinePaymentAsync(It.IsAny<InsertOfflinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            // Act & Assert
            var act = async () => await _service.OfflinePaymentAsync(request, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorInsertingOfflinePayment);

            // Verify log entry
            _loggerMock.VerifyLog(LogLevel.Error, LogMessages.UnexpectedErrorInsertingOfflinePayment, Times.Once());
        }

        [TestMethod]
        public async Task InitiateOfflinePayment_AmountIsZero_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OfflinePaymentRequestDto>().With(d => d.Amount, 0).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required and must be greater than zero.")
            };

            _offlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.OfflinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();

            Assert.Fail("is this test required - is 0 valid?");
        }
    }
}