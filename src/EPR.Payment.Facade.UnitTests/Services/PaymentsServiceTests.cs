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
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using EPR.Payment.Facade.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.UnitTests.Services
{
    [TestClass]
    public class PaymentsServiceTests
    {
        private IFixture? _fixture;
        private Mock<IHttpGovPayService>? _httpGovPayServiceMock;
        private Mock<IHttpPaymentsService>? _httpPaymentsServiceMock;
        private Mock<ILogger<PaymentsService>>? _loggerMock;
        private Mock<IOptions<PaymentServiceOptions>>? _optionsMock;
        private Mock<ICookieService>? _cookieServiceMock;
        private PaymentsService? _service;
        private IMapper? _mapper;

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

            _httpGovPayServiceMock = _fixture.Freeze<Mock<IHttpGovPayService>>();
            _httpPaymentsServiceMock = _fixture.Freeze<Mock<IHttpPaymentsService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<PaymentsService>>>();
            _optionsMock = _fixture.Freeze<Mock<IOptions<PaymentServiceOptions>>>();
            _cookieServiceMock = _fixture.Freeze<Mock<ICookieService>>();

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
                _cookieServiceMock.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_ValidRequest_ReturnsResponse(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> optionsMock,
            [Frozen] Mock<ICookieService> cookieServiceMock,
            PaymentsService service,
            PaymentRequestDto request,
            GovPayResponseDto expectedResponse)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "govPayPaymentId";

            expectedResponse.PaymentId = govPayPaymentId;
            expectedResponse.Links = new LinksDto
            {
                NextUrl = new LinkDto
                {
                    Href = "https://example.com/next"
                }
            };

            httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(externalPaymentId);
            httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()));

            // Act
            var result = await service.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken());

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull();
                result.NextUrl.Should().Be(expectedResponse.Links?.NextUrl?.Href);
                result.ExternalPaymentId.Should().Be(externalPaymentId);
                result.GovPayPaymentId.Should().Be(govPayPaymentId);
            }
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_NullRequest_ThrowsArgumentNullException(
            PaymentsService service)
        {
            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(null!, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow(null, "REF123", "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "Reg123", "Amount is required")]
        [DataRow(100, null, "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "Reg123", "Reference is required")]
        [DataRow(100, "REF123", null, "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "Reg123", "Organisation ID is required")]
        [DataRow(100, "REF123", "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", null, "Reg123", "User ID is required")]
        [DataRow(100, "REF123", "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", null, "Regulator is required")]
        [DataRow(0, "REF123", "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "Reg123", "Amount must be greater than zero")]
        [DataRow(-1, "REF123", "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "Reg123", "Amount must be greater than zero")]
        public async Task InitiatePayment_MissingFields_ThrowsValidationException(
            int? amount, string reference, string organisationId, string userId, string regulator, string expectedMessage)
        {
            // Arrange
            Guid? orgId = !string.IsNullOrEmpty(organisationId) ? Guid.Parse(organisationId) : (Guid?)null;
            Guid? userIdGuid = !string.IsNullOrEmpty(userId) ? Guid.Parse(userId) : (Guid?)null;

            var request = new PaymentRequestDto
            {
                Amount = amount,
                Reference = reference,
                OrganisationId = orgId,
                UserId = userIdGuid,
                Regulator = regulator
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();

            using (new FluentAssertions.Execution.AssertionScope())
            {
                exception.Which.Message.Should().Contain(expectedMessage);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_StatusUpdateValidationFails_ThrowsValidationException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            PaymentsService service,
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

            httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>().WithMessage("Validation error");
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
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            var httpGovPayServiceMock = fixture.Freeze<Mock<IHttpGovPayService>>();
            var httpPaymentsServiceMock = fixture.Freeze<Mock<IHttpPaymentsService>>();
            var loggerMock = fixture.Freeze<Mock<ILogger<PaymentsService>>>();
            var optionsMock = fixture.Freeze<Mock<IOptions<PaymentServiceOptions>>>();
            var cookieServiceMock = fixture.Freeze<Mock<ICookieService>>();

            optionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
            });
            var mapper = mapperConfig.CreateMapper();

            var service = new PaymentsService(
                httpGovPayServiceMock.Object,
                httpPaymentsServiceMock.Object,
                loggerMock.Object,
                optionsMock.Object,
                mapper,
                cookieServiceMock.Object);

            var completeRequest = fixture.Create<CompletePaymentRequestDto>();
            var paymentStatusResponse = fixture.Create<PaymentStatusResponseDto>();

            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = status, Finished = finished, Message = message, Code = code };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            var expectedUpdateRequest = new UpdatePaymentRequestDto
            {
                ExternalPaymentId = completeRequest.ExternalPaymentId,
                GovPayPaymentId = govPayPaymentId,
                UpdatedByUserId = completeRequest.UpdatedByUserId,
                UpdatedByOrganisationId = completeRequest.UpdatedByOrganisationId,
                Reference = paymentStatusResponse.Reference,
                Status = expectedStatus,
                ErrorCode = code,
                ErrorMessage = message
            };

            // Act
            var result = await service.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken());

            // Assert
            httpPaymentsServiceMock.Verify(s =>
                s.UpdatePaymentAsync(
                    completeRequest.ExternalPaymentId,
                    It.Is<UpdatePaymentRequestDto>(r =>
                        r.Status == expectedStatus &&
                        r.GovPayPaymentId == govPayPaymentId &&
                        r.UpdatedByUserId == completeRequest.UpdatedByUserId &&
                        r.UpdatedByOrganisationId == completeRequest.UpdatedByOrganisationId &&
                        r.Reference == paymentStatusResponse.Reference &&
                        r.ErrorCode == code &&
                        r.ErrorMessage == message),
                    It.IsAny<CancellationToken>()), Times.Once);

            result.Should().BeEquivalentTo(new CompletePaymentResponseDto
            {
                Status = expectedStatus,
                Message = message
            });
        }


        [TestMethod, AutoMoqData]
        public async Task CompletePayment_NullGovPayPaymentId_ThrowsArgumentException(
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(null!, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("GovPayPaymentId cannot be null or empty (Parameter 'govPayPaymentId')");
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_EmptyGovPayPaymentId_ThrowsArgumentException(
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(string.Empty, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("GovPayPaymentId cannot be null or empty (Parameter 'govPayPaymentId')");
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_PaymentStatusNotFound_ThrowsPaymentStatusNotFoundException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var govPayPaymentId = "12345";
            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentStatusResponseDto?)null);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_PaymentStateNull_ThrowsPaymentStatusNotFoundException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = null
            };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_PaymentStatusNull_ThrowsPaymentStatusNotFoundException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = null, Finished = true }
            };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            var act = async () => await service.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_StatusUpdateValidationFails_ThrowsValidationException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest,
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "error", Code = "Payment provider returned an error", Finished = true };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);
            httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>().WithMessage("Validation error");
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_StatusUpdateUnexpectedError_ThrowsUnexpectedErrorUpdatingPaymentException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest,
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "error", Code = "Payment provider returned an error", Finished = true };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);
            httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingPayment);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_ReturnUrlNotConfigured_ThrowsReturnUrlNotConfiguredException(
    [Frozen] Mock<IOptions<PaymentServiceOptions>> optionsMock,
    [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
    [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
    [Frozen] Mock<ILogger<PaymentsService>> loggerMock,
    [Frozen] Mock<ICookieService> cookieServiceMock,
    [Frozen] IMapper mapper,
    PaymentRequestDto request)
        {
            // Arrange: Explicitly create a PaymentServiceOptions object with ReturnUrl set to null
            var paymentServiceOptions = new PaymentServiceOptions { ReturnUrl = null, Description = "Payment description" };
            optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            // Instantiate the service with the mocked options
            var service = new PaymentsService(
                httpGovPayServiceMock.Object,
                httpPaymentsServiceMock.Object,
                loggerMock.Object,
                optionsMock.Object,
                mapper,
                cookieServiceMock.Object);

            // Debug: Check the configuration being returned
            var configuredOptions = optionsMock.Object.Value;
            Assert.IsNull(configuredOptions.ReturnUrl);  // Ensure ReturnUrl is null
            Assert.AreEqual("Payment description", configuredOptions.Description);  // Check Description

            // Act & Assert: Expect an InvalidOperationException due to missing ReturnUrl
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.ReturnUrlNotConfigured);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_DescriptionNotConfigured_ThrowsDescriptionNotConfiguredException(
            [Frozen] Mock<IOptions<PaymentServiceOptions>> optionsMock,
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> loggerMock,
            [Frozen] Mock<ICookieService> cookieServiceMock,
            [Frozen] IMapper mapper,
            PaymentRequestDto request)
        {
            // Arrange
            var paymentServiceOptions = new PaymentServiceOptions { ReturnUrl = "https://example.com/return", Description = null };
            optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            var service = new PaymentsService(
                httpGovPayServiceMock.Object,
                httpPaymentsServiceMock.Object,
                loggerMock.Object,
                optionsMock.Object,
                mapper,
                cookieServiceMock.Object);

            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.DescriptionNotConfigured);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_MissingUserId_ThrowsValidationException(
            PaymentsService service,
            PaymentRequestDto request)
        {
            // Arrange
            request.UserId = null;

            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>().WithMessage("User ID is required");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_MissingOrganisationId_ThrowsValidationException(
            PaymentsService service,
            PaymentRequestDto request)
        {
            // Arrange
            request.OrganisationId = null;

            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>().WithMessage("Organisation ID is required");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_MissingAmount_ThrowsValidationException(
            PaymentsService service,
            PaymentRequestDto request)
        {
            // Arrange
            request.Amount = null;

            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>().WithMessage("Amount is required");
        }

        [TestMethod, AutoMoqData]
        public async Task InsertPayment_ValidationExceptionThrown_LogsAndThrows(
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> loggerMock,
            PaymentsService service,
            PaymentRequestDto request)
        {
            // Arrange
            var validationException = new ValidationException(ExceptionMessages.ErrorInsertingPayment);
            httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act & Assert
            var exception = await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>();

            // Use a flexible matching to ensure the message contains the expected constant message
            using (new FluentAssertions.Execution.AssertionScope())
            {
                exception.Which.Message.Should().Match($"*{ExceptionMessages.ErrorInsertingPayment}*");
            }

            // Verify log entry
            loggerMock.VerifyLog(LogLevel.Error, LogMessages.ValidationErrorInsertingPayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task InsertPayment_UnexpectedExceptionThrown_LogsAndThrows(
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsService>> loggerMock,
            PaymentsService service,
            PaymentRequestDto request)
        {
            // Arrange
            var unexpectedException = new Exception("Unexpected error");
            httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            // Act & Assert
            var act = async () => await service.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken());
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorInsertingPayment);

            // Verify log entry
            loggerMock.VerifyLog(LogLevel.Error, LogMessages.UnexpectedErrorInsertingPayment, Times.Once());
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePaymentAsync_UnexpectedErrorDuringUpdatePayment_ThrowsAndLogsException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> paymentServiceOptionsMock,
            [Frozen] Mock<IMapper> mapperMock,
            [Frozen] Mock<ICookieService> cookieServiceMock,
            Mock<ILogger<PaymentsService>> loggerMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest,
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "error", Finished = true, Code = "P0050" }; // Ensure error code is provided

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            var unexpectedException = new Exception("Unexpected error");
            httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            // Ensure the service is constructed with the proper dependencies
            service = new PaymentsService(
                httpGovPayServiceMock.Object,
                httpPaymentsServiceMock.Object,
                loggerMock.Object,
                paymentServiceOptionsMock.Object,
                mapperMock.Object,
                cookieServiceMock.Object
            );

            // Act
            Func<Task> act = async () => await service.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken());

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingPayment);

            // Verify log entry
            loggerMock.VerifyLog(LogLevel.Error, LogMessages.UnexpectedErrorUpdatingPayment, Times.Once());
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_UpdatePaymentStatusUnexpectedError_ThrowsAndLogsException(
    [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
    [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
    [Frozen] Mock<IMapper> mapperMock,
    [Frozen] Mock<ICookieService> cookieServiceMock,
    Mock<ILogger<PaymentsService>> loggerMock,
    PaymentRequestDto request,
    GovPayResponseDto govPayResponse,
    CancellationToken cancellationToken)
        {
            // Arrange
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
            httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(unexpectedException);

            httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken))
                .ReturnsAsync(govPayResponse);

            var service = new PaymentsService(
                httpGovPayServiceMock.Object,
                httpPaymentsServiceMock.Object,
                loggerMock.Object,
                paymentServiceOptions,
                mapperMock.Object,
                cookieServiceMock.Object
            );

            // Act
            Func<Task> act = async () => await service.InitiatePaymentAsync(request, new DefaultHttpContext().Response, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.UnexpectedErrorUpdatingPayment);

            // Verify log entry
            loggerMock.Verify(
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
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> paymentServiceOptionsMock,
            [Frozen] Mock<IMapper> mapperMock,
            Mock<ILogger<PaymentsService>> loggerMock,
            PaymentsService service,
            PaymentRequestDto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            govPayResponse.PaymentId = null; // Simulate null PaymentId
            httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken)).ReturnsAsync(govPayResponse);
            paymentServiceOptionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            // Act
            Func<Task> act = async () => await service.InitiatePaymentAsync(request, new DefaultHttpContext().Response, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_GovPayResponsePaymentIdIsEmpty_ThrowsInvalidOperationException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> paymentServiceOptionsMock,
            [Frozen] Mock<IMapper> mapperMock,
            Mock<ILogger<PaymentsService>> loggerMock,
            PaymentsService service,
            PaymentRequestDto request,
            GovPayResponseDto govPayResponse,
            CancellationToken cancellationToken)
        {
            // Arrange
            govPayResponse.PaymentId = ""; // Simulate empty PaymentId
            httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), cancellationToken)).ReturnsAsync(govPayResponse);
            paymentServiceOptionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            // Act
            Func<Task> act = async () => await service.InitiatePaymentAsync(request, new DefaultHttpContext().Response, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_InvalidPaymentStatus_ThrowsException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest,
            PaymentStatusResponseDto paymentStatusResponse)
        {
            // Arrange
            var govPayPaymentId = "12345";
            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = "invalid_status", Finished = true };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_GovPayResponsePaymentIdIsNullOrEmpty_ThrowsInvalidOperationException(
    [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
    [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
    [Frozen] Mock<IOptions<PaymentServiceOptions>> paymentServiceOptionsMock,
    [Frozen] Mock<IMapper> mapperMock,
    [Frozen] Mock<ICookieService> cookieServiceMock,
    Mock<ILogger<PaymentsService>> loggerMock,
    PaymentsService service,
    PaymentRequestDto request)
        {
            // Arrange
            var govPayResponse = new GovPayResponseDto
            {
                PaymentId = null // Simulate null PaymentId
            };
            httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(govPayResponse);

            var paymentServiceOptions = new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            };
            paymentServiceOptionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            service = new PaymentsService(
                httpGovPayServiceMock.Object,
                httpPaymentsServiceMock.Object,
                loggerMock.Object,
                paymentServiceOptionsMock.Object,
                mapperMock.Object,
                cookieServiceMock.Object
            );

            // Act
            Func<Task> act = async () => await service.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);

            // Verify log entry for empty PaymentId
            govPayResponse.PaymentId = "";
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_GovPayResponsePaymentIdIsEmpty_ThrowsInvalidOperationException(
    [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
    [Frozen] Mock<IHttpPaymentsService> httpPaymentsServiceMock,
    [Frozen] Mock<IOptions<PaymentServiceOptions>> optionsMock,
    [Frozen] Mock<ICookieService> cookieServiceMock,
    PaymentRequestDto request)
        {
            // Arrange
            var govPayResponse = new GovPayResponseDto { PaymentId = string.Empty }; // Simulate empty PaymentId

            httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(govPayResponse);
            httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());
            optionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
            });
            var mapper = mapperConfig.CreateMapper();

            var service = new PaymentsService(
                httpGovPayServiceMock.Object,
                httpPaymentsServiceMock.Object,
                Mock.Of<ILogger<PaymentsService>>(),
                optionsMock.Object,
                mapper,
                cookieServiceMock.Object);

            // Act
            Func<Task> act = async () => await service.InitiatePaymentAsync(request, new DefaultHttpContext().Response, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ExceptionMessages.GovPayResponseInvalid);
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_AmountIsZero_ThrowsValidationException(
            PaymentsService service,
            PaymentRequestDto request)
        {
            // Arrange
            request.Amount = 0; // Invalid amount

            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>().WithMessage("Amount must be greater than zero");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_AmountIsNegative_ThrowsValidationException(
            PaymentsService service,
            PaymentRequestDto request)
        {
            // Arrange
            request.Amount = -10; // Invalid amount

            // Act & Assert
            await service.Invoking(async s => await s.InitiatePaymentAsync(request, new DefaultHttpContext().Response, new CancellationToken()))
                .Should().ThrowAsync<ValidationException>().WithMessage("Amount must be greater than zero");
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_SuccessStatusWithErrorCode_ThrowsException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = "success", Finished = true, Code = "SomeErrorCode" }
            };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<Exception>()
                .WithMessage(ExceptionMessages.SuccessStatusWithErrorCode);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_FailedStatusWithEmptyErrorCode_ThrowsException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = "failed", Finished = true, Code = null }
            };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.FailedStatusWithoutErrorCode);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_ErrorStatusWithEmptyErrorCode_ThrowsException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = "error", Finished = true, Code = null }
            };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorStatusWithoutErrorCode);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_UnknownStatus_ThrowsException(
            [Frozen] Mock<IHttpGovPayService> httpGovPayServiceMock,
            PaymentsService service,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = "unknown_status", Finished = true }
            };

            httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentStatusResponse);

            // Act & Assert
            await service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest, new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }
    }
}
