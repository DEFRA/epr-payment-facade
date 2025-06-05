using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers
{
    [TestClass]
    public class OfflinePaymentsControllerTests
    {
        private IFixture _fixture = null!;
        private OfflinePaymentsController _controller = null!;
        private Mock<IOfflinePaymentsService> _offlinePaymentsServiceMock = null!;
        private Mock<IValidator<OfflinePaymentRequestDto>> _offlinePaymentRequestValidatorMock = null!;
        private Mock<IValidator<OfflinePaymentRequestV2Dto>> _offlinePaymentRequestV2ValidatorMock = null!;
        private Mock<ILogger<OfflinePaymentsController>> _loggerMock = null!;
        private CancellationToken _cancellationToken;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _offlinePaymentsServiceMock = new Mock<IOfflinePaymentsService>();
            _offlinePaymentRequestValidatorMock = _fixture.Freeze<Mock<IValidator<OfflinePaymentRequestDto>>>();
            _offlinePaymentRequestV2ValidatorMock = _fixture.Freeze<Mock<IValidator<OfflinePaymentRequestV2Dto>>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<OfflinePaymentsController>>>();           
            _controller = new OfflinePaymentsController(_offlinePaymentsServiceMock.Object, _loggerMock.Object, _offlinePaymentRequestValidatorMock.Object, _offlinePaymentRequestV2ValidatorMock.Object);
            _cancellationToken = new CancellationToken();
        }

        [TestMethod]
        public void Constructor_WithValidArguments_ShouldInitializeCorrectly()
        {
            // Act
            var controller = new OfflinePaymentsController(_offlinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _offlinePaymentRequestValidatorMock.Object,
                _offlinePaymentRequestV2ValidatorMock.Object);

            // Assert
            using (new AssertionScope())
            {
                controller.Should().NotBeNull();
                controller.Should().BeAssignableTo<OfflinePaymentsController>();
            }
        }

        [TestMethod]
        public void Constructor_WhenOfflinePaymentsServiceIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IOfflinePaymentsService? offlinePaymentsServiceMock = null;

            // Act
            Action act = () => new OfflinePaymentsController(offlinePaymentsServiceMock!,
                                _loggerMock.Object,
                                _offlinePaymentRequestValidatorMock.Object,
                                _offlinePaymentRequestV2ValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'offlinePaymentsService')");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            ILogger<OfflinePaymentsController>? logger = null;

            // Act
            Action act = () => new OfflinePaymentsController(_offlinePaymentsServiceMock.Object,
                                logger!,
                                _offlinePaymentRequestValidatorMock.Object,
                                 _offlinePaymentRequestV2ValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'logger')");
        }

        [TestMethod]
        public void Constructor_WhenOfflinePaymentInsertRequestValidatorIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IValidator<OfflinePaymentRequestDto>? offlinePaymentRequestValidatorMock = null;

            // Act
            Action act = () => new OfflinePaymentsController(_offlinePaymentsServiceMock.Object!,
                                _loggerMock.Object,
                                offlinePaymentRequestValidatorMock!,
                                 _offlinePaymentRequestV2ValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'offlinePaymentRequestValidator')");
        }

        [TestMethod]
        public void Constructor_WhenOfflinePaymentInsertRequestV2ValidatorIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IValidator<OfflinePaymentRequestV2Dto>? offlinePaymentRequestV2ValidatorMock = null;

            // Act
            Action act = () => new OfflinePaymentsController(_offlinePaymentsServiceMock.Object!,
                                _loggerMock.Object,
                                _offlinePaymentRequestValidatorMock.Object,
                                 offlinePaymentRequestV2ValidatorMock!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'offlinePaymentRequestV2Validator')");
        }


        [TestMethod, AutoMoqData]
        public async Task InsertOfflinePayment_ValidInput_ShouldReturnOk()
        {
            // Arrange
            var request = _fixture.Build<OfflinePaymentRequestDto>().Create();

            //Act
            var result = await _controller.OfflinePayment(request, _cancellationToken);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOnlinePayment_RequestValidationFails_ShouldReturnsBadRequestWithValidationErrorDetails(
           [Frozen] OfflinePaymentRequestDto request)
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Reference", "Reference is required"),
                new ValidationFailure("Regulator", "Regulator is required")
            };

            _offlinePaymentRequestValidatorMock.Setup(v => v.Validate(It.IsAny<OfflinePaymentRequestDto>()))
                .Returns(new ValidationResult(validationFailures));

            // Act
            var result = await _controller.OfflinePayment(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
                var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Which;
                problemDetails.Detail.Should().Be("Reference is required; Regulator is required");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOfflinePayment_ServiceThrowsException_ShouldReturnInternalServerError([Frozen] OfflinePaymentRequestDto request)
        {
            // Arrange

            _offlinePaymentsServiceMock.Setup(service => service.OfflinePaymentAsync(request, _cancellationToken))
                               .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.OfflinePayment(request, _cancellationToken);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod]
        public async Task InsertOfflinePayment_ThrowsValidationException_ShouldReturnBadRequest()
        {
            // Arrange

            var request = _fixture.Build<OfflinePaymentRequestDto>().Create();

            var validationException = new ValidationException("Validation error");
            _offlinePaymentsServiceMock.Setup(s => s.OfflinePaymentAsync(It.IsAny<OfflinePaymentRequestDto>(), _cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await _controller.OfflinePayment(request, _cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();

                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult.Should().NotBeNull();
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOfflinePaymentV2_ValidInput_ShouldReturnOk()
        {
            // Arrange
            var request = _fixture.Build<OfflinePaymentRequestV2Dto>().Create();

            //Act
            var result = await _controller.OfflinePaymentV2(request, _cancellationToken);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOnlinePaymentV2_RequestValidationFails_ShouldReturnsBadRequestWithValidationErrorDetails()
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Reference", "Reference is required"),
                new ValidationFailure("Regulator", "Regulator is required"),
                new ValidationFailure("PaymentMethod", "The PaymentMethod field is required.")                
            };

            var request = _fixture.Build<OfflinePaymentRequestV2Dto>().Create();
            request.PaymentMethod = null;
            request.Reference = string.Empty;
            request.Regulator = string.Empty;


            _offlinePaymentRequestV2ValidatorMock.Setup(v => v.Validate(It.IsAny<OfflinePaymentRequestV2Dto>()))
                .Returns(new ValidationResult(validationFailures));

            // Act
            var result = await _controller.OfflinePaymentV2(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
                var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Which;
                problemDetails.Detail.Should().Be("Reference is required; Regulator is required; The PaymentMethod field is required.");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOfflinePaymentV2_ServiceThrowsException_ShouldReturnInternalServerError([Frozen] OfflinePaymentRequestV2Dto request)
        {
            // Arrange

            _offlinePaymentsServiceMock.Setup(service => service.OfflinePaymentAsync(request, _cancellationToken))
                               .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.OfflinePaymentV2(request, _cancellationToken);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod]
        public async Task InsertOfflinePaymentV2_ThrowsValidationException_ShouldReturnBadRequest()
        {
            // Arrange

            var request = _fixture.Build<OfflinePaymentRequestV2Dto>().Create();

            var validationException = new ValidationException("Validation error");
            _offlinePaymentsServiceMock.Setup(s => s.OfflinePaymentAsync(It.IsAny<OfflinePaymentRequestV2Dto>(), _cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await _controller.OfflinePaymentV2(request, _cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();

                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult.Should().NotBeNull();
            }
        }
    }
}