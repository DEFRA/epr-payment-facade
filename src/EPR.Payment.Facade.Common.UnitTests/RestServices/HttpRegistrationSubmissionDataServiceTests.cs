using System.Net;
using System.Text;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.UnitTests.RESTServices
{
    [TestClass]
    public class HttpRegistrationSubmissionDataServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptionsMonitor<Service>> _configMonitorMock = null!;
        private CreateRegistrationSubmissionDataRequest _request = null!;

        [TestInitialize]
        public void Init()
        {
            _configMonitorMock = new Mock<IOptionsMonitor<Service>>();
            _configMonitorMock.Setup(x => x.Get("RegistrationSubmissionDataService")).Returns(new Service
            {
                Url = "https://api.example.com",
                EndPointName = "api/v1",
            });
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _request = new CreateRegistrationSubmissionDataRequest
            {
                SubmissionId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                ComplianceSchemeId = Guid.NewGuid(),
                SubmissionPeriod = "Jan to Jun 2026",
                SubmissionDate = new DateTime(2026, 5, 28, 0, 0, 0, DateTimeKind.Utc),
            };
        }

        private HttpRegistrationSubmissionDataService Create(HttpClient client) =>
            new(client, _httpContextAccessorMock.Object, _configMonitorMock.Object);

        [TestMethod]
        public void Constructor_NullHttpContextAccessor_Throws()
        {
            Action act = () => _ = new HttpRegistrationSubmissionDataService(new HttpClient(), null!, _configMonitorMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContextAccessor");
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_Returns200_ReturnsParsedGuid([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            var expectedId = Guid.NewGuid();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(expectedId), Encoding.UTF8, "application/json"),
                });

            var sut = Create(new HttpClient(handlerMock.Object));

            var id = await sut.CreateAsync(_request, CancellationToken.None);

            id.Should().Be(expectedId);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_BadRequest_ThrowsValidationException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("\"bad request body\"", Encoding.UTF8, "application/json"),
                });

            var sut = Create(new HttpClient(handlerMock.Object));

            Func<Task> act = () => sut.CreateAsync(_request, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_500_ThrowsServiceException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("oops"),
                });

            var sut = Create(new HttpClient(handlerMock.Object));

            Func<Task> act = () => sut.CreateAsync(_request, CancellationToken.None);

            await act.Should().ThrowAsync<ServiceException>();
        }
    }
}
