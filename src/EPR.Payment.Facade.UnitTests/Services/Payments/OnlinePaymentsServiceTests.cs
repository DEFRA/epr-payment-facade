using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using AutoMapper;
using EPR.Payment.Common.Mapping;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.Common;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Enums.Payments;
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
    public class OnlinePaymentsServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpGovPayService> _httpGovPayServiceMock = null!;
        private Mock<IHttpOnlinePaymentsService> _httpOnlinePaymentsServiceMock = null!;
        private Mock<IHttpOnlinePaymentsV2Service> _httpOnlinePaymentsServiceMockV2 = null!;
        private Mock<ILogger<OnlinePaymentsService>> _loggerMock = null!;
        private Mock<IOptions<OnlinePaymentServiceOptions>> _optionsMock = null!;
        private OnlinePaymentsService _service = null!;
        private IMapper _mapper = null!;
        private Mock<IValidator<OnlinePaymentRequestDto>> _onlinePaymentRequestDtoMock = null!;
        private Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestV2DtoValidator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _onlinePaymentRequestDtoMock = new Mock<IValidator<OnlinePaymentRequestDto>>();
            _onlinePaymentRequestV2DtoValidator = new Mock<IValidator<OnlinePaymentRequestV2Dto>>();
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            var throwingRecursionBehaviors = _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList();
            foreach (var behavior in throwingRecursionBehaviors)
            {
                _fixture.Behaviors.Remove(behavior);
            }
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _httpGovPayServiceMock = _fixture.Freeze<Mock<IHttpGovPayService>>();
            _httpOnlinePaymentsServiceMock = _fixture.Freeze<Mock<IHttpOnlinePaymentsService>>();
            _httpOnlinePaymentsServiceMockV2 = _fixture.Freeze<Mock<IHttpOnlinePaymentsV2Service>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<OnlinePaymentsService>>>();
            _optionsMock = _fixture.Freeze<Mock<IOptions<OnlinePaymentServiceOptions>>>();

            _optionsMock.Setup(o => o.Value).Returns(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
                cfg.AddProfile<PaymentRequestMappingProfileV2>();
            });
            _mapper = mapperConfig.CreateMapper();

            _service = new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceMockV2.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _mapper,
                _onlinePaymentRequestDtoMock.Object,
                _onlinePaymentRequestV2DtoValidator.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenAllDependenciesAreNotNull_ShouldCreateInstance(
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsService> _httpOnlinePaymentsServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsV2Service> _httpOnlinePaymentsServiceV2Mock,
            [Frozen] Mock<ILogger<OnlinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestDto>> _onlinePaymentRequestDtoValidatorMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestDtoValidatorV2Mock)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            var service = new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceV2Mock.Object,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _onlinePaymentRequestDtoValidatorMock.Object,
                _onlinePaymentRequestDtoValidatorV2Mock.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpGovPayServiceIsNull_ShouldThrowArgumentNullException(
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpOnlinePaymentsService> _httpOnlinePaymentsServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsV2Service> _httpOnlinePaymentsServiceV2Mock,
            [Frozen] Mock<ILogger<OnlinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestDto>> _onlinePaymentRequestDtoValidatorMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestDtoValidatorV2Mock)
        {
            // Arrange
            _onlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            Action act = () => new OnlinePaymentsService(
                null!,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceV2Mock.Object,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _onlinePaymentRequestDtoValidatorMock.Object,
                _onlinePaymentRequestDtoValidatorV2Mock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpGovPayService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpOnlinePaymentsServiceIsNull_ShouldThrowArgumentNullException(
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<ILogger<OnlinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IHttpOnlinePaymentsV2Service> _httpOnlinePaymentsServiceV2Mock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestDto>> _onlinePaymentRequestDtoValidatorMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestDtoValidatorV2Mock)
        {
            // Arrange
            _onlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            Action act = () => new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                null!,
                _httpOnlinePaymentsServiceV2Mock.Object,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _onlinePaymentRequestDtoValidatorMock.Object,
                _onlinePaymentRequestDtoValidatorV2Mock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpOnlinePaymentsService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpV2OnlinePaymentsServiceIsNull_ShouldThrowArgumentNullException(
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpOnlinePaymentsService> _httpOnlinePaymentsServiceMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<ILogger<OnlinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestDto>> _onlinePaymentRequestDtoValidatorMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestDtoValidatorV2Mock)
        {
            // Arrange
            _onlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            Action act = () => new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                null!,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _onlinePaymentRequestDtoValidatorMock.Object,
                _onlinePaymentRequestDtoValidatorV2Mock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpOnlinePaymentsV2Service");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException(
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsService> _httpOnlinePaymentsServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsV2Service> _httpOnlinePaymentsServiceV2Mock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestDto>> _onlinePaymentRequestDtoValidatorMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestDtoValidatorV2Mock)
        {
            // Arrange
            _onlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            Action act = () => new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceV2Mock.Object,
                null!,
                _onlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _onlinePaymentRequestDtoValidatorMock.Object,
                _onlinePaymentRequestDtoValidatorV2Mock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenOnlinePaymentServiceOptionsIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsService> _httpOnlinePaymentsServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsV2Service> _httpOnlinePaymentsServiceV2Mock,
            [Frozen] Mock<ILogger<OnlinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestDto>> _onlinePaymentRequestDtoValidatorMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestDtoValidatorV2Mock)
        {
            // Arrange
            _onlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns((OnlinePaymentServiceOptions)null!);

            // Act
            Action act = () => new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceV2Mock.Object,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                _onlinePaymentRequestDtoValidatorMock.Object, _onlinePaymentRequestDtoValidatorV2Mock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("onlinePaymentServiceOptions");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenMapperIsNull_ShouldThrowArgumentNullException(
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsService> _httpOnlinePaymentsServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsV2Service> _httpOnlinePaymentsServiceV2Mock,
            [Frozen] Mock<ILogger<OnlinePaymentsService>> _loggerMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestDto>> _onlinePaymentRequestDtoValidatorMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestDtoValidatorV2Mock)
        {
            // Arrange
            _onlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            Action act = () => new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceV2Mock.Object,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object,
                null!,
                _onlinePaymentRequestDtoValidatorMock.Object,
                _onlinePaymentRequestDtoValidatorV2Mock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenOnlinePaymentRequestDtoValidatorIsNull_ShouldThrowArgumentNullException(
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] Mock<IHttpOnlinePaymentsV2Service> _httpOnlinePaymentsServiceV2Mock,
            [Frozen] Mock<IHttpGovPayService> _httpGovPayServiceMock,
            [Frozen] Mock<IHttpOnlinePaymentsService> _httpOnlinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OnlinePaymentsService>> _loggerMock,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] Mock<IValidator<OnlinePaymentRequestV2Dto>> _onlinePaymentRequestDtoValidatorV2Mock)
        {
            // Arrange
            _onlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            Action act = () => new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceV2Mock.Object,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object,
                _mapperMock.Object,
                null!, _onlinePaymentRequestDtoValidatorV2Mock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("onlinePaymentRequestDtoValidator");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_ValidRequest_ReturnsResponse(
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
            _httpOnlinePaymentsServiceMock.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(externalPaymentId);
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()));


            var request = _fixture.Build<OnlinePaymentRequestDto>().With(d => d.UserId, new Guid()).With(x => x.OrganisationId, new Guid()).With(x => x.Regulator, RegulatorConstants.GBENG).Create();
            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            // Act
            var result = await _service.InitiateOnlinePaymentAsync(request, new CancellationToken());

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
        public async Task InitiateOnlinePayment_NullRequest_ThrowsArgumentNullException(
            OnlinePaymentsService service)
        {
            // Act & Assert
            await service.Invoking(async s => await s.InitiateOnlinePaymentAsync(null!, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiateOnlinePayment_MissingFields_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestDto>().With(d => d.UserId, (Guid?)null).With(d => d.OrganisationId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.UserId), "User ID is required."),
                new ValidationFailure(nameof(request.OrganisationId), "Organisation ID is required.")
            };

            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiateOnlinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_StatusUpdateValidationFails_ThrowsValidationException(
            OnlinePaymentRequestDto request,
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

            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            _httpOnlinePaymentsServiceMock.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(externalPaymentId);
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentAsync(request, new CancellationToken()))
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
        public async Task CompleteOnlinePayment_UpdatesPaymentStatus(
            string govPayPaymentId,
            string status,
            bool finished,
            string message,
            string code,
            PaymentStatus expectedStatus)
        {
            // Arrange

            _optionsMock.Setup(o => o.Value).Returns(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
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

            var paymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDetails);

            // Act
            var result = await _service.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken());

            // Assert
            _httpOnlinePaymentsServiceMock.Verify(s =>
                s.UpdateOnlinePaymentAsync(
                    externalPaymentId,
                    It.Is<UpdateOnlinePaymentRequestDto>(r =>
                        r.Status == expectedStatus &&
                        r.GovPayPaymentId == govPayPaymentId &&
                        r.Reference == paymentStatusResponse.Reference &&
                        r.ErrorCode == code &&
                        r.ErrorMessage == message),
                    It.IsAny<CancellationToken>()), Times.Once);

            result.Should().BeEquivalentTo(new CompleteOnlinePaymentResponseDto
            {
                Status = expectedStatus,
                Message = message,
                Reference = paymentStatusResponse.Reference,
                UserId = paymentDetails.UpdatedByUserId,
                OrganisationId = paymentDetails.UpdatedByOrganisationId,
                Regulator = paymentDetails.Regulator,
                Amount = paymentDetails.Amount,
                Email = paymentStatusResponse.Email,
                Description = paymentDetails.Description
            });
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_NullExternalPaymentId_ThrowsArgumentException(
            OnlinePaymentsService service)
        {
            // Act & Assert
            await service.Invoking(async s => await s.CompleteOnlinePaymentAsync(Guid.Empty, new CancellationToken()))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("ExternalPaymentId cannot be empty (Parameter 'externalPaymentId')");
        }

        [TestMethod]
        public async Task CompleteOnlinePayment_PaymentStatusNotFound_ThrowsPaymentStatusNotFoundException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = null }
            };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await _service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
        }


        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_PaymentStateNull_ThrowsPaymentStatusNotFoundException(
            OnlinePaymentsService service)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
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

            _httpOnlinePaymentsServiceMock?.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod]
        public async Task CompleteOnlinePayment_PaymentStatusNull_ThrowsPaymentStatusNotFoundException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
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

            _httpOnlinePaymentsServiceMock?.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            var act = async () => await _service.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_StatusUpdateValidationFails_ThrowsValidationException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
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
            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            await _service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<ServiceException>().WithMessage("Validation error");
        }


        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_StatusUpdateUnexpectedError_ThrowsUnexpectedErrorUpdatingPaymentException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
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
            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            await _service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingOnlinePayment);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_ReturnUrlNotConfigured_ThrowsReturnUrlNotConfiguredException(
            OnlinePaymentRequestDto request)
        {
            // Arrange
            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            var paymentServiceOptions = new OnlinePaymentServiceOptions
            {
                ReturnUrl = null, // ReturnUrl is not configured
            };

            _optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            var service = new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceMockV2.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _mapper,
                _onlinePaymentRequestDtoMock.Object,
                _onlinePaymentRequestV2DtoValidator.Object);
            // Act & Assert
            await service.Invoking(async s => await s.InitiateOnlinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(ExceptionMessages.ReturnUrlNotConfigured);
        }

        [TestMethod]
        public async Task InitiateOnlinePayment_MissingUserId_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestDto>().With(d => d.UserId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.UserId), "User ID is required.")
            };

            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiateOnlinePayment_MissingOrganisationId_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestDto>().With(d => d.OrganisationId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.OrganisationId), "Organisation ID is required.")
            };

            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiateOnlinePayment_MissingAmount_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestDto>().With(d => d.Amount, (int?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required.")
            };

            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOnlinePayment_ValidationExceptionThrown_LogsAndThrows(
            OnlinePaymentRequestDto request)
        {
            // Arrange
            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var validationException = new ValidationException(ExceptionMessages.ErrorInsertingOnlinePayment);
            _httpOnlinePaymentsServiceMock.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act & Assert
            var exception = await _service.Invoking(async s => await s.InitiateOnlinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ServiceException>();

            // Use a flexible matching to ensure the message contains the expected constant message
            using (new AssertionScope())
            {
                exception.Which.Message.Should().Match($"*{ExceptionMessages.ErrorInsertingOnlinePayment}*");
            }

            // Verify log entry
            _loggerMock.VerifyLog(LogLevel.Error, LogMessages.ValidationErrorInsertingOnlinePayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOnlinePayment_UnexpectedExceptionThrown_LogsAndThrows(
            OnlinePaymentRequestDto request)
        {
            // Arrange
            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var unexpectedException = new Exception("Unexpected error");
            _httpOnlinePaymentsServiceMock.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            // Act & Assert
            var act = async () => await _service.InitiateOnlinePaymentAsync(request, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorInsertingOnlinePayment);

            // Verify log entry
            _loggerMock.VerifyLog(LogLevel.Error, LogMessages.UnexpectedErrorInsertingOnlinePayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePaymentAsync_UnexpectedErrorDuringUpdateOnlinePayment_ThrowsAndLogsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "error", Finished = true, Code = "P0050" }; // Ensure error code is provided

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            var unexpectedException = new Exception("Unexpected error");
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            var loggerMock = new Mock<ILogger<OnlinePaymentsService>>();
            _service = new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceMockV2.Object,
                loggerMock.Object,
                Options.Create(new OnlinePaymentServiceOptions { ReturnUrl = "https://example.com/return" }),
                new MapperConfiguration(cfg => cfg.AddProfile<PaymentRequestMappingProfile>()).CreateMapper(),
                _onlinePaymentRequestDtoMock.Object,_onlinePaymentRequestV2DtoValidator.Object);

            // Act
            Func<Task> act = async () => await _service.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken());

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingOnlinePayment);

            // Verify log entry
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.UnexpectedErrorUpdatingOnlinePayment)),
                    It.Is<Exception>(e => e == unexpectedException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePaymentAsync_UpdateOnlinePaymentStatusUnexpectedError_ThrowsAndLogsException(
            [Frozen] Mock<IMapper> mapperMock,
            OnlinePaymentRequestDto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var onlinePaymentServiceOptions = Options.Create(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
            });

            mapperMock.Setup(m => m.Map<UpdateOnlinePaymentRequestDto>(request)).Returns(new UpdateOnlinePaymentRequestDto
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
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            _httpOnlinePaymentsServiceMock.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken))
                .ReturnsAsync(govPayResponse);

            // Act
            Func<Task> act = async () => await _service.InitiateOnlinePaymentAsync(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingOnlinePayment);

            // Verify log entry
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.UnexpectedErrorUpdatingOnlinePayment)),
                    It.Is<Exception>(e => e == unexpectedException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePaymentAsync_GovPayResponsePaymentIdIsNull_ThrowsInvalidOperationException(
            OnlinePaymentRequestDto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            govPayResponse.PaymentId = null; // Simulate null PaymentId
            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken)).ReturnsAsync(govPayResponse);
            _optionsMock.Setup(o => o.Value).Returns(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
            });

            // Act
            Func<Task> act = async () => await _service.InitiateOnlinePaymentAsync(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePaymentAsync_GovPayResponsePaymentIdIsEmpty_ThrowsInvalidOperationException(
            OnlinePaymentRequestDto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            govPayResponse.PaymentId = ""; // Simulate empty PaymentId
            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken)).ReturnsAsync(govPayResponse);
            _optionsMock.Setup(o => o.Value).Returns(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
            });

            // Act
            Func<Task> act = async () => await _service.InitiateOnlinePaymentAsync(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_InvalidPaymentStatus_ThrowsException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            OnlinePaymentsService service,
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "invalid_status", Finished = true };

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock?.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePaymentAsync_GovPayResponsePaymentIdIsNullOrEmpty_ThrowsInvalidOperationException(
            OnlinePaymentRequestDto request)
        {
            // Arrange
            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var govPayResponse = new GovPayResponseDto
            {
                PaymentId = null // Simulate null PaymentId
            };
            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(govPayResponse);

            var onlinePaymentServiceOptions = new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
            };
            _optionsMock.Setup(o => o.Value).Returns(onlinePaymentServiceOptions);

            // Act
            Func<Task> act = async () => await _service.InitiateOnlinePaymentAsync(request, new CancellationToken());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);

            // Verify log entry for empty PaymentId
            govPayResponse.PaymentId = "";
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod]
        public async Task InitiateOnlinePayment_AmountIsZero_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestDto>().With(d => d.Amount, 0).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required and must be greater than zero.")
            };

            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiateOnlinePayment_AmountIsNegative_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestDto>().With(d => d.Amount, -10).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required and must be greater than zero.")
            };

            _onlinePaymentRequestDtoMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentAsync(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_SuccessStatusWithErrorCode_ThrowsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
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
            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            // Act & Assert
            await _service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.SuccessStatusWithErrorCode);
        }


        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_FailedStatusWithEmptyErrorCode_ThrowsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "failed", Finished = true, Code = null };

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            Func<Task> act = async () => await _service.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken());

            var exception = await act.Should().ThrowAsync<Exception>();
            exception.WithMessage(ExceptionMessages.FailedStatusWithoutErrorCode);
        }


        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_ErrorStatusWithEmptyErrorCode_ThrowsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "error", Finished = true, Code = null };

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            Func<Task> act = async () => await _service.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken());

            var exception = await act.Should().ThrowAsync<Exception>();
            exception.WithMessage(ExceptionMessages.ErrorStatusWithoutErrorCode);
        }




        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_UnknownStatus_ThrowsException(
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "unknown_status", Finished = true };

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock?.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await _service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_GovPayPaymentIdIsNull_ThrowsException(
            OnlinePaymentsService service)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = null, // Simulating a null GovPayPaymentId
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            // Act & Assert
            await service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod]
        public async Task CompleteOnlinePayment_GovPayPaymentIdIsEmpty_ThrowsException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = string.Empty,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);

            // Act & Assert
            await _service.Invoking(async s => await s.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_GetOnlinePaymentDetailsThrowsException_LogsAndThrowsException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var expectedException = new Exception(ExceptionMessages.ErrorGettingOnlinePaymentDetails);

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var act = async () => await _service.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingOnlinePaymentDetails);

            // Verify that the error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.ErrorGettingOnlinePaymentDetails)),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task CompleteOnlinePayment_GetOnlinePaymentStatusThrowsException_LogsAndThrowsException()
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "12345";
            var expectedException = new Exception(ExceptionMessages.ErrorRetrievingPaymentStatus);

            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                GovPayPaymentId = govPayPaymentId,
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            _httpOnlinePaymentsServiceMock.Setup(s => s.GetOnlinePaymentDetailsAsync(externalPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(onlinePaymentDetails);
            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var act = async () => await _service.CompleteOnlinePaymentAsync(externalPaymentId, new CancellationToken());
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

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePayment_ValidRequest_ReturnsResponse(
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

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestV2Dto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            _httpOnlinePaymentsServiceMockV2.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestV2Dto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(externalPaymentId);
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()));


            var request = _fixture.Build<OnlinePaymentRequestV2Dto>().With(d => d.UserId, new Guid()).With(x => x.OrganisationId, new Guid()).With(x => x.Regulator, RegulatorConstants.GBENG).With(x => x.RequestorType, PaymentsRequestorTypes.Reprocessors).Create();
            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            // Act
            var result = await _service.InitiateOnlinePaymentV2Async(request, new CancellationToken());

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.NextUrl.Should().Be(expectedResponse.Links?.NextUrl?.Href);

                // Verify that the return_url contains the correct id
                _httpGovPayServiceMock.Verify(s =>
                    s.InitiatePaymentAsync(It.Is<GovPayRequestV2Dto>(r =>
                        r.return_url == $"https://example.com/return?id={externalPaymentId}"), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePayment_NullRequest_ThrowsArgumentNullException(
            OnlinePaymentsService service)
        {
            // Act & Assert
            await service.Invoking(async s => await s.InitiateOnlinePaymentV2Async(null!, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiateV2OnlinePayment_MissingFields_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestV2Dto>().With(d => d.UserId, (Guid?)null).With(d => d.OrganisationId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.UserId), "User ID is required."),
                new ValidationFailure(nameof(request.OrganisationId), "Organisation ID is required.")
            };

            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiateOnlinePaymentV2Async(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePayment_StatusUpdateValidationFails_ThrowsValidationException(
            OnlinePaymentRequestV2Dto request,
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

            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestV2Dto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            _httpOnlinePaymentsServiceMockV2.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestV2Dto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(externalPaymentId);
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentV2Async(request, new CancellationToken()))
                .Should().ThrowAsync<ServiceException>().WithMessage("Validation error");

            // Verify that the return_url contains the correct id
            _httpGovPayServiceMock.Verify(s =>
                s.InitiatePaymentAsync(It.Is<GovPayRequestV2Dto>(r =>
                    r.return_url == $"https://example.com/return?id={externalPaymentId}"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePayment_ReturnUrlNotConfigured_ThrowsReturnException(
            OnlinePaymentRequestV2Dto request)
        {
            // Arrange
            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());

            var paymentServiceOptions = new OnlinePaymentServiceOptions
            {
                ReturnUrl = null, // ReturnUrl is not configured
            };

            _optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            var service = new OnlinePaymentsService(
                _httpGovPayServiceMock.Object,
                _httpOnlinePaymentsServiceMock.Object,
                _httpOnlinePaymentsServiceMockV2.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _mapper,
                _onlinePaymentRequestDtoMock.Object,
                _onlinePaymentRequestV2DtoValidator.Object);
            // Act & Assert
            await service.Invoking(async s => await s.InitiateOnlinePaymentV2Async(request, new CancellationToken()))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(ExceptionMessages.ReturnUrlNotConfigured);
        }

        [TestMethod]
        public async Task InitiateV2OnlinePayment_MissingUserId_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestV2Dto>().With(d => d.UserId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.UserId), "User ID is required.")
            };

            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentV2Async(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiateV2OnlinePayment_MissingOrganisationId_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestV2Dto>().With(d => d.OrganisationId, (Guid?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.OrganisationId), "Organisation ID is required.")
            };

            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentV2Async(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod]
        public async Task InitiateV2OnlinePayment_MissingAmount_ThrowsValidationException()
        {
            // Arrange
            var request = _fixture.Build<OnlinePaymentRequestV2Dto>().With(d => d.Amount, (int?)null).Create();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.Amount), "Amount is required.")
            };

            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiateOnlinePaymentV2Async(request, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task InsertV2OnlinePayment_ValidationExceptionThrown_LogsAndThrows(
           OnlinePaymentRequestV2Dto request)
        {
            // Arrange
            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var validationException = new ValidationException(ExceptionMessages.ErrorInsertingOnlinePayment);
            _httpOnlinePaymentsServiceMockV2.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestV2Dto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act & Assert
            var exception = await _service.Invoking(async s => await s.InitiateOnlinePaymentV2Async(request, new CancellationToken()))
                .Should().ThrowAsync<ServiceException>();

            // Use a flexible matching to ensure the message contains the expected constant message
            using (new AssertionScope())
            {
                exception.Which.Message.Should().Match($"*{ExceptionMessages.ErrorInsertingOnlinePayment}*");
            }

            // Verify log entry
            _loggerMock.VerifyLog(LogLevel.Error, LogMessages.ValidationErrorInsertingOnlinePayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task InsertV2OnlinePayment_UnexpectedExceptionThrown_LogsAndThrows(
            OnlinePaymentRequestV2Dto request)
        {
            // Arrange
            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var unexpectedException = new Exception("Unexpected error");
            _httpOnlinePaymentsServiceMockV2.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestV2Dto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            // Act & Assert
            var act = async () => await _service.InitiateOnlinePaymentV2Async(request, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorInsertingOnlinePayment);

            // Verify log entry
            _loggerMock.VerifyLog(LogLevel.Error, LogMessages.UnexpectedErrorInsertingOnlinePayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePaymentAsync_UpdateOnlinePaymentStatusUnexpectedError_ThrowsAndLogsException(
            [Frozen] Mock<IMapper> mapperMock,
            OnlinePaymentRequestV2Dto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            var onlinePaymentServiceOptions = Options.Create(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
            });

            mapperMock.Setup(m => m.Map<UpdateOnlinePaymentRequestDto>(request)).Returns(new UpdateOnlinePaymentRequestDto
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
            _httpOnlinePaymentsServiceMock.Setup(s => s.UpdateOnlinePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdateOnlinePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            _httpOnlinePaymentsServiceMockV2.Setup(s => s.InsertOnlinePaymentAsync(It.IsAny<InsertOnlinePaymentRequestV2Dto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestV2Dto>(), cancellationToken))
                .ReturnsAsync(govPayResponse);

            // Act
            Func<Task> act = async () => await _service.InitiateOnlinePaymentV2Async(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingOnlinePayment);

            // Verify log entry
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.UnexpectedErrorUpdatingOnlinePayment)),
                    It.Is<Exception>(e => e == unexpectedException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePaymentAsync_GovPayResponsePaymentIdIsNull_ThrowsInvalidOperationException(
            OnlinePaymentRequestV2Dto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            govPayResponse.PaymentId = null; // Simulate null PaymentId
            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestV2Dto>(), cancellationToken)).ReturnsAsync(govPayResponse);
            _optionsMock.Setup(o => o.Value).Returns(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
            });

            // Act
            Func<Task> act = async () => await _service.InitiateOnlinePaymentV2Async(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePaymentAsync_GovPayResponsePaymentIdIsEmpty_ThrowsInvalidOperationException(
            OnlinePaymentRequestV2Dto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            _onlinePaymentRequestV2DtoValidator.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
            govPayResponse.PaymentId = ""; // Simulate empty PaymentId
            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestV2Dto>(), cancellationToken)).ReturnsAsync(govPayResponse);
            _optionsMock.Setup(o => o.Value).Returns(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return"
            });

            // Act
            Func<Task> act = async () => await _service.InitiateOnlinePaymentV2Async(request, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }
    }
}