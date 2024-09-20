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
    public class PaymentsServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpGovPayService> _httpGovPayServiceMock = null!;
        private Mock<IHttpPaymentsService> _httpPaymentsServiceMock = null!;
        private Mock<ILogger<PaymentsService>> _loggerMock = null!;
        private Mock<IOptions<PaymentServiceOptions>> _optionsMock = null!;
        private PaymentsService _service = null!;
        private IMapper _mapper = null!;
        private Mock<IValidator<PaymentRequestDto>> _paymentRequestDtoMock = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _paymentRequestDtoMock = new Mock<IValidator<PaymentRequestDto>>();
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            var throwingRecursionBehaviors = _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList();
            foreach (var behavior in throwingRecursionBehaviors)
            {
                _fixture.Behaviors.Remove(behavior);
            }
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _httpGovPayServiceMock = _fixture.Freeze<Mock<IHttpGovPayService>>();
            _httpPaymentsServiceMock = _fixture.Freeze<Mock<IHttpPaymentsService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<PaymentsService>>>();
            _optionsMock = _fixture.Freeze<Mock<IOptions<PaymentServiceOptions>>>();

            _optionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _mapper,
                _paymentRequestDtoMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenAllDependenciesAreNotNull_ShouldCreateInstance(
            [Frozen] PaymentServiceOptions _paymentServiceOptions,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> _httpPaymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<PaymentRequestDto>> _paymentRequestDtoValidatorMock)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns(_paymentServiceOptions);

            // Act
            var service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _paymentRequestDtoValidatorMock.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpGovPayServiceIsNull_ShouldThrowArgumentNullException(
            [Frozen] PaymentServiceOptions _paymentServiceOptions,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] Mock<IHttpPaymentsService> _httpPaymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<PaymentRequestDto>> _paymentRequestDtoValidatorMock)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns(_paymentServiceOptions);

            // Act
            Action act = () => new PaymentsService(
                null!,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _paymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpGovPayService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpPaymentsServiceIsNull_ShouldThrowArgumentNullException(
            [Frozen] PaymentServiceOptions _paymentServiceOptions,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<PaymentRequestDto>> _paymentRequestDtoValidatorMock)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns(_paymentServiceOptions);

            // Act
            Action act = () => new PaymentsService(
                _httpGovPayServiceMock.Object,
                null!,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _paymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpPaymentsService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException(
            [Frozen] PaymentServiceOptions _paymentServiceOptions,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> _httpPaymentsServiceMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<PaymentRequestDto>> _paymentRequestDtoValidatorMock)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns(_paymentServiceOptions);

            // Act
            Action act = () => new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                null!,
                _paymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _paymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenPaymentServiceOptionsIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> _httpPaymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<PaymentRequestDto>> _paymentRequestDtoValidatorMock)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns((PaymentServiceOptions)null!);

            // Act
            Action act = () => new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _paymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("paymentServiceOptions");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenMapperIsNull_ShouldThrowArgumentNullException(
            [Frozen] PaymentServiceOptions _paymentServiceOptions,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> _httpPaymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> _loggerMock,
            [Frozen] Mock<IValidator<PaymentRequestDto>> _paymentRequestDtoValidatorMock)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns(_paymentServiceOptions);

            // Act
            Action act = () => new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object,
                null!,
                _paymentRequestDtoValidatorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenPaymentRequestDtoValidatorIsNull_ShouldThrowArgumentNullException(
            [Frozen] PaymentServiceOptions _paymentServiceOptions,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> _httpPaymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns(_paymentServiceOptions);

            // Act
            Action act = () => new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object,
                _mapperMock.Object,
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("paymentRequestDtoValidator");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_ValidRequest_ReturnsResponse(
            GovPayResponseDto expectedResponse)
        {
            // Arrange
            expectedResponse.Links = new LinksDto
            {
                NextUrl = new LinkDto
                {
                    Href = "https://example.com/response"
                }
            };

            var externalPaymentId = Guid.NewGuid();

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            _httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(externalPaymentId);
            _httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()));


            var request = _fixture.Build<PaymentRequestDto>().With(d => d.UserId, new Guid()).With(x => x.OrganisationId, new Guid()).With(x => x.Regulator, RegulatorConstants.GBENG).Create();
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            // Act
            var result = await _service.InitiatePaymentAsync(request, new CancellationToken());

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.NextUrl.Should().Be(expectedResponse.Links?.NextUrl?.Href);

                // Verify that the return_url contains the correct id
                _httpGovPayServiceMock.Verify(s =>
                    s.InitiatePaymentAsync(It.Is<GovPayRequestDto>(r =>
                        r.return_url == $"https://example.com/return?id={externalPaymentId}"), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_NullRequest_ThrowsArgumentNullException(
            PaymentsService service)
        {
            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(null!, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiatePayment_MissingFields_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<PaymentRequestDto>().With(d => d.UserId, (Guid?)null).With(d => d.OrganisationId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.UserId), "User ID is required."),
                new ValidationFailure(nameof(request.OrganisationId), "Organisation ID is required.")
            };

            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_StatusUpdateValidationFails_ThrowsValidationException(
            PaymentRequestDto request,
            GovPayResponseDto expectedResponse)
        {
            // Arrange
            expectedResponse.Links = new LinksDto
            {
                NextUrl = new LinkDto
                {
                    Href = "https://example.com/response"
                }
            };

            var externalPaymentId = Guid.NewGuid();

            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            _httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(externalPaymentId);
            _httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ServiceException>().WithMessage("Validation error");

            // Verify that the return_url contains the correct id
            _httpGovPayServiceMock.Verify(s =>
                s.InitiatePaymentAsync(It.Is<GovPayRequestDto>(r =>
                    r.return_url == $"https://example.com/return?id={externalPaymentId}"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [DataRow("u4g456vpdeamkg72ihjt129io6", "success", true, null, null, PaymentStatus.Success, DisplayName = "CompletePayment_SuccessStatus_UpdatesPaymentStatus")]
        [DataRow("ubs761va12akse548htbu2oie5", "failed", true, "Payment method rejected", "P0010", PaymentStatus.Failed, DisplayName = "CompletePayment_CardDeclined_UpdatesPaymentStatus")]
        [DataRow("mpni2094aqjf89q5k2rdveed2o", "failed", true, "Payment method rejected", "P0010", PaymentStatus.Failed, DisplayName = "CompletePayment_CardExpired_UpdatesPaymentStatus")]
        [DataRow("ke41o2t96h1983os203no7q5so", "failed", true, "Payment method rejected", "P0010", PaymentStatus.Failed, DisplayName = "CompletePayment_InvalidCVC_UpdatesPaymentStatus")]
        [DataRow("jgpldh7b1i5qmh59ru6ia67386", "error", true, "Payment provider returned an error", "P0050", PaymentStatus.Error, DisplayName = "CompletePayment_GeneralError_UpdatesPaymentStatus")]
        [DataRow("n9nvasa4782ogtuh19e8mum68r", "failed", true, "Payment was cancelled by the user", "P0030", PaymentStatus.UserCancelled, DisplayName = "CompletePayment_UserCancelled_UpdatesPaymentStatus")]
        public async Task CompletePayment_UpdatesPaymentStatus(
            string govPayPaymentId,
            string status,
            bool finished,
            string message,
            string code,
            PaymentStatus expectedStatus)
        {
            // Arrange

            _optionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
            });

            var externalPaymentId = Guid.NewGuid();
            var paymentStatusResponse = _fixture.Create<PaymentStatusResponseDto>();

            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = status, Finished = finished, Message = message, Code = code };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            // Act
            var result = await _service.CompletePaymentAsync(externalPaymentId, new CancellationToken());

            // Assert
            _httpPaymentsServiceMock.Verify(s =>
                s.UpdatePaymentAsync(
                    externalPaymentId,
                    It.Is<UpdatePaymentRequestDto>(r =>
                        r.Status == expectedStatus &&
                        r.GovPayPaymentId == govPayPaymentId &&
                        r.Reference == paymentStatusResponse.Reference &&
                        r.ErrorCode == code &&
                        r.ErrorMessage == message),
                    It.IsAny<CancellationToken>()), Times.Once);

            result.Should().BeEquivalentTo(new CompletePaymentResponseDto
            {
                Status = expectedStatus,
                Message = message,
                Reference = paymentStatusResponse.Reference,
                UserId = paymentDetails.UpdatedByUserId,
                OrganisationId = paymentDetails.UpdatedByOrganisationId,
                Regulator = paymentDetails.Regulator,
                Amount = paymentDetails.Amount,
                Email = paymentStatusResponse.Email
            });

            Nullable.GetUnderlyingType(result.GetType().GetProperty(nameof(result.Amount)).PropertyType).Should().BeNull();
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_NullExternalPaymentId_ThrowsArgumentException(
            PaymentsService service)
        {
            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(Guid.Empty, new CancellationToken()))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("ExternalPaymentId cannot be empty (Parameter 'externalPaymentId')");
        }

        [TestMethod]
        public async Task CompletePayment_PaymentStatusNotFound_ThrowsPaymentStatusNotFoundException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = null }
            };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
        }


        [TestMethod, AutoMoqData]
        public async Task CompletePayment_PaymentStateNull_ThrowsPaymentStatusNotFoundException(
            PaymentsService service)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = null
            };

            _httpPaymentsServiceMock?.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod]
        public async Task CompletePayment_PaymentStatusNull_ThrowsPaymentStatusNotFoundException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = null, Finished = true }
            };

            _httpPaymentsServiceMock?.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            var act = async () => await _service.CompletePaymentAsync(externalPaymentId, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_StatusUpdateValidationFails_ThrowsValidationException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "failed", Code = "SomeErrorCode", Finished = true };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);
            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);
            _httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<ServiceException>().WithMessage("Validation error");
        }


        [TestMethod, AutoMoqData]
        public async Task CompletePayment_StatusUpdateUnexpectedError_ThrowsUnexpectedErrorUpdatingPaymentException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "error", Code = "P0050", Finished = true };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);
            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);
            _httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingPayment);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_ReturnUrlNotConfigured_ThrowsReturnUrlNotConfiguredException(
            PaymentRequestDto request)
        {
            // Arrange
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            var paymentServiceOptions = new PaymentServiceOptions
            {
                ReturnUrl = null, // ReturnUrl is not configured
                Description = "Payment description"
            };

            _optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            var service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _mapper,
                _paymentRequestDtoMock.Object);
            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(ExceptionMessages.ReturnUrlNotConfigured);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_DescriptionNotConfigured_ThrowsDescriptionNotConfiguredException(
            PaymentRequestDto request)
        {
            // Arrange
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var paymentServiceOptions = new PaymentServiceOptions { ReturnUrl = "https://example.com/return", Description = null };
            _optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            var service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _mapper,
                _paymentRequestDtoMock.Object);

            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.DescriptionNotConfigured);
        }

        [TestMethod]
        public async Task InitiatePayment_MissingUserId_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<PaymentRequestDto>().With(d => d.UserId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.UserId), "User ID is required.")
            };

            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiatePayment_MissingOrganisationId_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<PaymentRequestDto>().With(d => d.OrganisationId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.OrganisationId), "Organisation ID is required.")
            };

            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiatePayment_MissingAmount_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<PaymentRequestDto>().With(d => d.Amount, (int?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required.")
            };

            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task InsertPayment_ValidationExceptionThrown_LogsAndThrows(
            PaymentRequestDto request)
        {
            // Arrange
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var validationException = new ValidationException(ExceptionMessages.ErrorInsertingPayment);
            _httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act & Assert
            var exception = await _service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ServiceException>();

            // Use a flexible matching to ensure the message contains the expected constant message
            using (new AssertionScope())
            {
                exception.Which.Message.Should().Match($"*{ExceptionMessages.ErrorInsertingPayment}*");
            }

            // Verify log entry
            _loggerMock.VerifyLog(LogLevel.Error, LogMessages.ValidationErrorInsertingPayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task InsertPayment_UnexpectedExceptionThrown_LogsAndThrows(
            PaymentRequestDto request)
        {
            // Arrange
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var unexpectedException = new Exception("Unexpected error");
            _httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            // Act & Assert
            var act = async () => await _service.InitiatePaymentAsync(request, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorInsertingPayment);

            // Verify log entry
            _loggerMock.VerifyLog(LogLevel.Error, LogMessages.UnexpectedErrorInsertingPayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePaymentAsync_UnexpectedErrorDuringUpdatePayment_ThrowsAndLogsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "error", Finished = true, Code = "P0050" }; // Ensure error code is provided

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            var unexpectedException = new Exception("Unexpected error");
            _httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            var loggerMock = new Mock<ILogger<PaymentsService>>();
            _service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                loggerMock.Object,
                Options.Create(new PaymentServiceOptions { ReturnUrl = "https://example.com/return", Description = "Payment description" }),
                new MapperConfiguration(cfg => cfg.AddProfile<PaymentRequestMappingProfile>()).CreateMapper(),
                _paymentRequestDtoMock.Object);

            // Act
            Func<Task> act = async () => await _service.CompletePaymentAsync(externalPaymentId, new CancellationToken());

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingPayment);

            // Verify log entry
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.UnexpectedErrorUpdatingPayment)),
                    It.Is<Exception>(e => e == unexpectedException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_UpdatePaymentStatusUnexpectedError_ThrowsAndLogsException(
            [Frozen] Mock<IMapper> mapperMock,
            PaymentRequestDto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var paymentServiceOptions = Options.Create(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            mapperMock.Setup(m => m.Map<UpdatePaymentRequestDto>(request)).Returns(new UpdatePaymentRequestDto
            {
                ExternalPaymentId = Guid.NewGuid(),
                GovPayPaymentId = "govPayPaymentId",
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid(),
                Reference = "reference",
                Status = PaymentStatus.InProgress,
                ErrorMessage = null,
                ErrorCode = null
            });

            govPayResponse.PaymentId = "govPayPaymentId";
            govPayResponse.Links = new LinksDto { NextUrl = new LinkDto { Href = "nextUrl" } };

            var unexpectedException = new Exception("Unexpected error");
            _httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            _httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken))
                .ReturnsAsync(govPayResponse);

            // Act
            Func<Task> act = async () => await _service.InitiatePaymentAsync(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingPayment);

            // Verify log entry
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.UnexpectedErrorUpdatingPayment)),
                    It.Is<Exception>(e => e == unexpectedException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_GovPayResponsePaymentIdIsNull_ThrowsInvalidOperationException(
            PaymentRequestDto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            govPayResponse.PaymentId = null; // Simulate null PaymentId
            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken)).ReturnsAsync(govPayResponse);
            _optionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            // Act
            Func<Task> act = async () => await _service.InitiatePaymentAsync(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_GovPayResponsePaymentIdIsEmpty_ThrowsInvalidOperationException(
            PaymentRequestDto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            govPayResponse.PaymentId = ""; // Simulate empty PaymentId
            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken)).ReturnsAsync(govPayResponse);
            _optionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            // Act
            Func<Task> act = async () => await _service.InitiatePaymentAsync(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_InvalidPaymentStatus_ThrowsException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "invalid_status", Finished = true };

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock?.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_GovPayResponsePaymentIdIsNullOrEmpty_ThrowsInvalidOperationException(
            PaymentRequestDto request)
        {
            // Arrange
            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var govPayResponse = new GovPayResponseDto
            {
                PaymentId = null // Simulate null PaymentId
            };
            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(govPayResponse);

            var paymentServiceOptions = new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            };
            _optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            // Act
            Func<Task> act = async () => await _service.InitiatePaymentAsync(request, new CancellationToken());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);

            // Verify log entry for empty PaymentId
            govPayResponse.PaymentId = "";
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod]
        public async Task InitiatePayment_AmountIsZero_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<PaymentRequestDto>().With(d => d.Amount, 0).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required and must be greater than zero.")
            };

            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiatePayment_AmountIsNegative_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<PaymentRequestDto>().With(d => d.Amount, -10).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required and must be greater than zero.")
            };

            _paymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_SuccessStatusWithErrorCode_ThrowsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "success", Code = "SomeErrorCode", Finished = true };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);
            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.SuccessStatusWithErrorCode);
        }


        [TestMethod, AutoMoqData]
        public async Task CompletePayment_FailedStatusWithEmptyErrorCode_ThrowsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "failed", Finished = true, Code = null };

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            Func<Task> act = async () => await _service.CompletePaymentAsync(externalPaymentId, new CancellationToken());

            var exception = await act.Should().ThrowAsync<Exception>();
            exception.WithMessage(ExceptionMessages.FailedStatusWithoutErrorCode);
        }


        [TestMethod, AutoMoqData]
        public async Task CompletePayment_ErrorStatusWithEmptyErrorCode_ThrowsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "error", Finished = true, Code = null };

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            Func<Task> act = async () => await _service.CompletePaymentAsync(externalPaymentId, new CancellationToken());

            var exception = await act.Should().ThrowAsync<Exception>();
            exception.WithMessage(ExceptionMessages.ErrorStatusWithoutErrorCode);
        }




        [TestMethod, AutoMoqData]
        public async Task CompletePayment_UnknownStatus_ThrowsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "unknown_status", Finished = true };

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock?.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_GovPayPaymentIdIsNull_ThrowsException(
            PaymentsService service)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = null, // Simulating a null GovPayPaymentId
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod]
        public async Task CompletePayment_GovPayPaymentIdIsEmpty_ThrowsException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = string.Empty,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_GetPaymentDetailsThrowsException_LogsAndThrowsException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var expectedException = new Exception(ExceptionMessages.ErrorGettingPaymentDetails);

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var act = async () => await _service.CompletePaymentAsync(externalPaymentId, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentDetails);

            // Verify that the error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.ErrorGettingPaymentDetails)),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task CompletePayment_GetPaymentStatusThrowsException_LogsAndThrowsException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            var expectedException = new Exception(ExceptionMessages.ErrorRetrievingPaymentStatus);

            var paymentDetails = new PaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpPaymentsServiceMock.Setup(s => s.GetPaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);
            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var act = async () => await _service.CompletePaymentAsync(externalPaymentId, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);

            // Verify that the error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.ErrorRetrievingPaymentStatus)),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

    }
}