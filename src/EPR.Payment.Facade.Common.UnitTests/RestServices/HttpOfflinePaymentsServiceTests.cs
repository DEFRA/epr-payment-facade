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
    public class HttpOfflinePaymentsServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;
        private InsertOfflinePaymentRequestDto _insertOfflinePaymentRequestDto = null!;
        private Guid _paymentId;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "offlinepayments",
                HttpClientName = "HttpClientName"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _insertOfflinePaymentRequestDto = new InsertOfflinePaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                Reference = "123456",
                Amount = 100,
                Description = "Payment description"
            };

            _paymentId = Guid.NewGuid();
        }

        private HttpOfflinePaymentsService CreateHttpOfflinePaymentsService(HttpClient httpClient)
        {
            return new HttpOfflinePaymentsService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task InsertOfflinePaymentAsync_Success_ReturnsGuid(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpOfflinePaymentsService httpOfflinePaymentsService,
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
            httpOfflinePaymentsService = CreateHttpOfflinePaymentsService(httpClient);

            // Act
            var result = await httpOfflinePaymentsService.InsertOfflinePaymentAsync(_insertOfflinePaymentRequestDto, cancellationToken);

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
            HttpOfflinePaymentsService httpOfflinePaymentsService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorInsertingOfflinePayment));

            var httpClient = new HttpClient(handlerMock.Object);
            httpOfflinePaymentsService = CreateHttpOfflinePaymentsService(httpClient);

            // Act
            Func<Task> act = async () => await httpOfflinePaymentsService.InsertOfflinePaymentAsync(_insertOfflinePaymentRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorInsertingOfflinePayment);
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
