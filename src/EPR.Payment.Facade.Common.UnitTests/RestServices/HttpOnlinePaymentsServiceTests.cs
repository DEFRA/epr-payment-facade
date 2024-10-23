using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace EPR.Payment.Facade.Common.UnitTests.RESTServices
{
    [TestClass]
    public class HttpOnlinePaymentsServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;
        private InsertOnlinePaymentRequestDto _insertOnlinePaymentRequestDto = null!;
        private UpdateOnlinePaymentRequestDto _updateOnlinePaymentRequestDto = null!;
        private Guid _paymentId;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "online-payments",
                HttpClientName = "HttpClientName"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _insertOnlinePaymentRequestDto = new InsertOnlinePaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Reference = "123456",
                Regulator = "Regulator",
                Amount = 100,
                ReasonForPayment = "Payment description",
                Status = Common.Enums.PaymentStatus.Initiated
            };
            _updateOnlinePaymentRequestDto = new UpdateOnlinePaymentRequestDto
            {
                ExternalPaymentId = Guid.NewGuid(),
                GovPayPaymentId = "govpay123",
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid(),
                Reference = "123456",
                Status = Common.Enums.PaymentStatus.InProgress
            };
            _paymentId = Guid.NewGuid();
        }

        private HttpOnlinePaymentsService CreateHttpOnlinePaymentsService(HttpClient httpClient)
        {
            return new HttpOnlinePaymentsService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOnlinePaymentAsync_Success_ReturnsGuid(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpOnlinePaymentsService httpOnlinePaymentsService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_paymentId), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpOnlinePaymentsService = CreateHttpOnlinePaymentsService(httpClient);

            // Act
            var result = await httpOnlinePaymentsService.InsertOnlinePaymentAsync(_insertOnlinePaymentRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().Be(_paymentId);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOnlinePaymentAsync_Failure_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpOnlinePaymentsService httpOnlinePaymentsService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorInsertingOnlinePayment));

            var httpClient = new HttpClient(handlerMock.Object);
            httpOnlinePaymentsService = CreateHttpOnlinePaymentsService(httpClient);

            // Act
            Func<Task> act = async () => await httpOnlinePaymentsService.InsertOnlinePaymentAsync(_insertOnlinePaymentRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorInsertingOnlinePayment);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }

        }

        [TestMethod, AutoMoqData]
        public async Task UpdateOnlinePaymentAsync_Success_UpdatesPaymentStatus(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpOnlinePaymentsService httpOnlinePaymentsService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpOnlinePaymentsService = CreateHttpOnlinePaymentsService(httpClient);

            // Act
            await httpOnlinePaymentsService.UpdateOnlinePaymentAsync(_paymentId, _updateOnlinePaymentRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task UpdateOnlinePayment_Failure_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpOnlinePaymentsService httpOnlinePaymentsService,
            CancellationToken cancellationToken)
        {
            // Arrange
            var updateOnlinePaymentRequestDto = _updateOnlinePaymentRequestDto!;

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorUpdatingOnlinePayment));

            var httpClient = new HttpClient(handlerMock.Object);
            httpOnlinePaymentsService = CreateHttpOnlinePaymentsService(httpClient);

            // Act
            Func<Task> act = async () => await httpOnlinePaymentsService.UpdateOnlinePaymentAsync(updateOnlinePaymentRequestDto.ExternalPaymentId, updateOnlinePaymentRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorUpdatingOnlinePayment);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>());
            }

        }

        [TestMethod, AutoMoqData]
        public async Task GetOnlinePaymentDetailsAsync_Success_ReturnsPaymentDetailsDto(
    [Frozen] Mock<HttpMessageHandler> handlerMock,
    HttpOnlinePaymentsService httpOnlinePaymentsService,
    CancellationToken cancellationToken)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var onlinePaymentDetails = new OnlinePaymentDetailsDto
            {
                ExternalPaymentId = externalPaymentId,
                GovPayPaymentId = "govpay123",
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(onlinePaymentDetails), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpOnlinePaymentsService = CreateHttpOnlinePaymentsService(httpClient);

            // Act
            var result = await httpOnlinePaymentsService.GetOnlinePaymentDetailsAsync(externalPaymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(onlinePaymentDetails);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetOnlinePaymentDetailsAsync_Failure_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpOnlinePaymentsService httpOnlinePaymentsService,
            CancellationToken cancellationToken)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorGettingOnlinePaymentDetails));

            var httpClient = new HttpClient(handlerMock.Object);
            httpOnlinePaymentsService = CreateHttpOnlinePaymentsService(httpClient);

            // Act
            Func<Task> act = async () => await httpOnlinePaymentsService.GetOnlinePaymentDetailsAsync(externalPaymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorGettingOnlinePaymentDetails);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

    }
}
