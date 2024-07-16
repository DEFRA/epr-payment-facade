using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
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
    public class HttpPaymentsServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;
        private InsertPaymentRequestDto _insertPaymentRequestDto = null!;
        private UpdatePaymentRequestDto _updatePaymentRequestDto = null!;
        private Guid _paymentId;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "payments",
                HttpClientName = "HttpClientName"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _insertPaymentRequestDto = new InsertPaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Reference = "123456",
                Regulator = "Regulator",
                Amount = 100,
                ReasonForPayment = "Payment description",
                Status = Common.Enums.PaymentStatus.Initiated
            };
            _updatePaymentRequestDto = new UpdatePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                GovPayPaymentId = "govpay123",
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid(),
                Reference = "123456",
                Status = Common.Enums.PaymentStatus.InProgress
            };
            _paymentId = Guid.NewGuid();
        }

        private HttpPaymentsService CreateHttpPaymentsService(HttpClient httpClient)
        {
            return new HttpPaymentsService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task InsertPaymentAsync_Success_ReturnsGuid(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpPaymentsService httpPaymentsService,
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
            httpPaymentsService = CreateHttpPaymentsService(httpClient);

            // Act
            var result = await httpPaymentsService.InsertPaymentAsync(_insertPaymentRequestDto, cancellationToken);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().Be(_paymentId);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InsertPaymentAsync_Failure_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpPaymentsService httpPaymentsService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorInsertingPayment));

            var httpClient = new HttpClient(handlerMock.Object);
            httpPaymentsService = CreateHttpPaymentsService(httpClient);

            // Act & Assert
            Func<Task> act = async () => await httpPaymentsService.InsertPaymentAsync(_insertPaymentRequestDto, cancellationToken);
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorInsertingPayment);
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentAsync_Success_UpdatesPaymentStatus(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpPaymentsService httpPaymentsService,
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
            httpPaymentsService = CreateHttpPaymentsService(httpClient);

            // Act
            await httpPaymentsService.UpdatePaymentAsync(_paymentId, _updatePaymentRequestDto, cancellationToken);

            // Assert
            // No exception means success
            using (new FluentAssertions.Execution.AssertionScope())
            {
                // Any additional assertions for success can be added here
            }
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePayment_Failure_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpPaymentsService httpPaymentsService,
            CancellationToken cancellationToken)
        {
            // Arrange
            var updatePaymentRequestDto = _updatePaymentRequestDto!;

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorUpdatingPayment));

            var httpClient = new HttpClient(handlerMock.Object);
            httpPaymentsService = CreateHttpPaymentsService(httpClient);

            // Act & Assert
            Func<Task> act = async () => await httpPaymentsService.UpdatePaymentAsync(updatePaymentRequestDto.Id, updatePaymentRequestDto, cancellationToken);
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorUpdatingPayment);
        }
    }

    public class HttpClientFactoryMock : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public HttpClientFactoryMock(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}
