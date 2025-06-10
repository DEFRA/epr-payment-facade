using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Enums.Payments;
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
    public class HttpOnlinePaymentsV2ServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptionsMonitor<Service>> _configMonitorMock = null!;
        private InsertOnlinePaymentRequestV2Dto _insertOnlinePaymentRequestDto = null!;
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

            _configMonitorMock = new Mock<IOptionsMonitor<Service>>();
            _configMonitorMock.Setup(x => x.Get("OnlineV2PaymentService")).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _insertOnlinePaymentRequestDto = new InsertOnlinePaymentRequestV2Dto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Reference = "123456",
                Regulator = "Regulator",
                Amount = 100,
                ReasonForPayment = "Payment description",
                Status = Common.Enums.PaymentStatus.Initiated,
                RequestorType = PaymentsRequestorTypes.Producers
            };
            _paymentId = Guid.NewGuid();
        }

        private HttpOnlinePaymentsV2Service CreateHttpOnlinePaymentsService(HttpClient httpClient)
        {
            return new HttpOnlinePaymentsV2Service(
                httpClient,
                _httpContextAccessorMock!.Object,
                _configMonitorMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOnlinePaymentAsync_Success_ReturnsGuid(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpOnlinePaymentsV2Service httpOnlinePaymentsService,
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
            HttpOnlinePaymentsV2Service httpOnlinePaymentsService,
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

    }
}
