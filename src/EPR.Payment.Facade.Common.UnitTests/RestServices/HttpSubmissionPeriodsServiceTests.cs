using System.Net;
using System.Text;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.UnitTests.RESTServices
{
    [TestClass]
    public class HttpSubmissionPeriodsServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptionsMonitor<Service>> _configMonitorMock = null!;

        [TestInitialize]
        public void Init()
        {
            _configMonitorMock = new Mock<IOptionsMonitor<Service>>();
            _configMonitorMock.Setup(x => x.Get("SubmissionPeriodsService")).Returns(new Service
            {
                Url = "https://api.example.com",
                EndPointName = "api/v1/submission-periods",
            });
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        }

        private HttpSubmissionPeriodsService Create(HttpClient client) =>
            new(client, _httpContextAccessorMock.Object, _configMonitorMock.Object);

        [TestMethod]
        public void Constructor_NullHttpContextAccessor_Throws()
        {
            Action act = () => _ = new HttpSubmissionPeriodsService(new HttpClient(), null!, _configMonitorMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContextAccessor");
        }

        [TestMethod, AutoMoqData]
        public async Task GetAllAsync_Returns200_ReturnsParsedList([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            var expected = new List<SubmissionPeriodResponseDto>
            {
                new() { Id = 1, WindowType = "Cso", RegistrationYear = 2025 },
                new() { Id = 3, WindowType = "CsoLargeProducer", RegistrationYear = 2026 },
            };
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(expected), Encoding.UTF8, "application/json"),
                });

            var sut = Create(new HttpClient(handlerMock.Object));

            var result = await sut.GetAllAsync(CancellationToken.None);

            result.Should().BeEquivalentTo(expected);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod, AutoMoqData]
        public async Task GetAllAsync_500_ThrowsServiceException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("oops"),
                });

            var sut = Create(new HttpClient(handlerMock.Object));

            Func<Task> act = () => sut.GetAllAsync(CancellationToken.None);

            await act.Should().ThrowAsync<ServiceException>();
        }
    }
}
