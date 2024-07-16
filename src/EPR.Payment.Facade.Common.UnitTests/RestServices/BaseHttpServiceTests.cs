using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Payment.Facade.Common.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace EPR.Payment.Facade.Common.UnitTests.RestServices
{
    [TestClass]
    public class BaseHttpServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
        private Mock<HttpMessageHandler> _handlerMock = null!;
        private HttpClient _httpClient = null!;
        private TestableBaseHttpService _testableHttpService = null!;
        private IFixture _fixture = null!;

        private const string baseUrl = "http://paymentfacadedummy.com";
        private const string endPointName = "api";
        private const string url = "paymentfacadeurl";
        private string expectedUrl = string.Empty;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _httpContextAccessorMock = _fixture.Freeze<Mock<IHttpContextAccessor>>();
            _httpClientFactoryMock = _fixture.Freeze<Mock<IHttpClientFactory>>();
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _httpClient = new HttpClient(_handlerMock.Object);
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            expectedUrl = $"{baseUrl}/{endPointName}/{url}/";

            _testableHttpService = new TestableBaseHttpService(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                baseUrl,
                endPointName);
        }

        [TestMethod]
        public async Task Post_Should_Call_Send_With_Correct_Parameters_And_Return_Result()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;
            var responseContent = "{\"result\": \"success\"}";

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                })
                .Verifiable();

            // Act
            var result = await _testableHttpService.PublicPost<object>(url, payload, cancellationToken);

            // Assert
            result.Should().NotBeNull();

            // Parse the result to JToken and assert the expected JSON token
            var jsonResult = JToken.Parse(result!.ToString()!);
            jsonResult["result"]!.ToString().Should().Be("success");

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post &&
                    msg.RequestUri!.ToString() == expectedUrl &&
                    msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public void SetBearerToken_Should_Set_Authorization_Header()
        {
            // Arrange
            var token = "testToken";

            // Act
            _testableHttpService.PublicSetBearerToken(token);

            // Assert
            _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            _httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
            _httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(token);
        }

        [TestMethod]
        public void Constructor_Should_Throw_ArgumentNullException_When_HttpContextAccessor_Is_Null()
        {
            // Act
            Action act = () => new TestableBaseHttpService(null!, _httpClientFactoryMock.Object, baseUrl, endPointName);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*httpContextAccessor*");
        }

        [TestMethod]
        public void Constructor_Should_Throw_ArgumentNullException_When_HttpClientFactory_Is_Null()
        {
            // Act
            Action act = () => new TestableBaseHttpService(_httpContextAccessorMock.Object, null!, baseUrl, endPointName);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*httpClientFactory*");
        }

        [TestMethod]
        public void Constructor_Should_Throw_ArgumentNullException_When_BaseUrl_Is_Null()
        {
            // Act
            Action act = () => new TestableBaseHttpService(_httpContextAccessorMock.Object, _httpClientFactoryMock.Object, null!, endPointName);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*baseUrl*");
        }

        [TestMethod]
        public void Constructor_Should_Throw_ArgumentNullException_When_EndPointName_Is_Null()
        {
            // Act
            Action act = () => new TestableBaseHttpService(_httpContextAccessorMock.Object, _httpClientFactoryMock.Object, baseUrl, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*endPointName*");
        }

        [TestMethod]
        public async Task Post_Should_Throw_ResponseCodeException_When_Response_Is_Unsuccessful()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicPost<object>(url, payload, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<ResponseCodeException>()
                .WithMessage("*Bad Request*");

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post &&
                    msg.RequestUri!.ToString() == expectedUrl &&
                    msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                ItExpr.IsAny<CancellationToken>());
        }
    }

}
