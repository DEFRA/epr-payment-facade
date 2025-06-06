using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Common.Enums.Payments;
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
    public class HttpOfflinePaymentsServiceV2Tests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;        
        private OfflinePaymentRequestV2Dto _offlinePaymentRequestV2Dto = null!;
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

            _offlinePaymentRequestV2Dto = new OfflinePaymentRequestV2Dto
            {
                UserId = Guid.NewGuid(),
                Reference = "123456",
                Regulator = RegulatorConstants.GBENG,
                Amount = 100,
                Description = RegulatorConstants.GBENG,
                PaymentMethod = OfflinePaymentMethodTypes.Cheque
            };

            _paymentId = Guid.NewGuid();
        }

        private HttpOfflinePaymentsServiceV2 CreateHttpOfflinePaymentsServiceV2(HttpClient httpClient)
        {
            // Mock IOptionsMonitor<Service>
            var optionsMonitorMock = new Mock<IOptionsMonitor<Service>>();

            // Mock the Get method to return the expected service configuration
            optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_configMock.Object.Value);
            optionsMonitorMock.Setup(x => x.Get(It.IsAny<string>())).Returns(_configMock.Object.Value);

            return new HttpOfflinePaymentsServiceV2(
                httpClient,
                _httpContextAccessorMock.Object,
                optionsMonitorMock.Object
            );
        }       

        [TestMethod, AutoMoqData]
        public async Task InsertOfflinePaymentV2Async_Success_ExecutesSuccessfully(
   [Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();  // Use a fresh cancellation token
            var cancellationToken = cancellationTokenSource.Token;  // Ensure it's not cancelled prematurely

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>(
                           "SendAsync",
                           ItExpr.IsAny<HttpRequestMessage>(),
                           ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_paymentId), Encoding.UTF8, "application/json")
                       })
                       .Callback<HttpRequestMessage, CancellationToken>((msg, token) =>
                       {
                           // Verify cancellation token is not triggered during the test
                           Assert.IsFalse(token.IsCancellationRequested, "Cancellation token was triggered unexpectedly.");
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            var httpOfflinePaymentsServiceV2 = CreateHttpOfflinePaymentsServiceV2(httpClient);

            // Act
            await httpOfflinePaymentsServiceV2.InsertOfflinePaymentAsync(_offlinePaymentRequestV2Dto, cancellationToken);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post &&
                    msg.RequestUri!.ToString() == "https://example.com/offlinepayments/offline-payments/"), // Update this to the correct full URL
                ItExpr.IsAny<CancellationToken>());
        }


        [TestMethod, AutoMoqData]
        public async Task InsertOfflinePaymentV2Async_Failure_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>(
                           "SendAsync",
                           ItExpr.IsAny<HttpRequestMessage>(),
                           ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorInsertingOfflinePayment));

            var httpClient = new HttpClient(handlerMock.Object);
            var httpOfflinePaymentsServiceV2 = CreateHttpOfflinePaymentsServiceV2(httpClient);

            // Act
            Func<Task> act = async () => await httpOfflinePaymentsServiceV2.InsertOfflinePaymentAsync(_offlinePaymentRequestV2Dto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                // Verify that the request was sent and that it is using the correct URL
                await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorInsertingOfflinePayment);

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post &&
                        msg.RequestUri!.ToString() == "https://example.com/offlinepayments/offline-payments/"), // Update this to the correct full URL
                    ItExpr.IsAny<CancellationToken>());
            }
        }

    }
}
