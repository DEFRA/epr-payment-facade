using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Payment.Facade.Common.Exceptions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
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
                _httpClient,
                _httpContextAccessorMock.Object,
                baseUrl);
        }

        [TestMethod]
        public async Task Get_ShouldCallSendWithCorrectParametersAndReturnResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var responseContent = "{\"result\": \"success\"}";

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                })
                .Verifiable();

            // Act
            var result = await _testableHttpService.PublicGet<object>(url, cancellationToken);
            // Parse the result to JToken and assert the expected JSON token
            var jsonResult = JToken.Parse(result!.ToString()!);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                jsonResult["result"]!.ToString().Should().Be("success");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get &&
                        msg.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>());
            }

        }

        [TestMethod]
        public async Task Post_ShouldCallSendWithCorrectParametersAndReturnResult()
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
            // Parse the result to JToken and assert the expected JSON token
            var jsonResult = JToken.Parse(result!.ToString()!);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
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

        }

        [TestMethod]
        public async Task PostNonGeneric_ShouldCallSendWithCorrectParametersAndReturnResult()
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
            await _testableHttpService.PublicPost(url, payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }

        }

        [TestMethod]
        public async Task Put_ShouldCallSendWithCorrectParametersAndReturnResult()
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
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                })
                .Verifiable();

            // Act
            var result = await _testableHttpService.PublicPut<object>(url, payload, cancellationToken);
            // Parse the result to JToken and assert the expected JSON token
            var jsonResult = JToken.Parse(result!.ToString()!);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                jsonResult["result"]!.ToString().Should().Be("success");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Put &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }

        }

        [TestMethod]
        public async Task PutNonGeneric_ShouldCallSendWithCorrectParametersAndReturnResult()
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
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                })
                .Verifiable();

            // Act
            await _testableHttpService.PublicPut(url, payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Put &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }

        }

        [TestMethod]
        public async Task Delete_ShouldCallSendWithCorrectParametersAndReturnResult()
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
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString() == expectedUrl &&
                        req.Content != null &&
                        req.Content.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                })
                .Verifiable();

            // Act
            var result = await _testableHttpService.PublicDelete<object>(url, payload, cancellationToken);
            var jsonResult = JToken.Parse(result!.ToString()!); // Parse the result for verification

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                jsonResult["result"]!.ToString().Should().Be("success");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Delete &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task DeleteNonGeneric_ShouldCallSendWithCorrectParametersAndReturnResult()
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
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                })
                .Verifiable();

            // Act
            await _testableHttpService.PublicDelete(url, payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Delete &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }

        }

        [TestMethod]
        public async Task Get_WhenResponseIsUnsuccessful_ShouldThrowResponseCodeException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicGet<object>(url, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ResponseCodeException>()
                    .WithMessage("*Bad Request*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get &&
                        msg.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task Get_WhenUrlIsEmpty_ShouldThrowException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicGet<object>("", cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<Exception>();

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get &&
                        msg.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task Post_WhenResponseIsUnsuccessful_ShouldThrowResponseCodeException()
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
            using (new AssertionScope())
            {
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

        [TestMethod]
        public async Task Post_WhenUrlIsEmpty_ShouldThrowArgumentNullException()
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
            Func<Task> act = async () => await _testableHttpService.PublicPost<object>("", payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ArgumentNullException>()
                    .WithMessage("*Value cannot be null*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task PostNonGeneric_WhenResponseIsUnsuccessful_ShouldThrowResponseCodeException()
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
            Func<Task> act = async () => await _testableHttpService.PublicPost(url, payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
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

        [TestMethod]
        public async Task PostNonGeneric_WhenUrlIsEmpty_ShouldThrowArgumentNullException()
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
            Func<Task> act = async () => await _testableHttpService.PublicPost("", payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ArgumentNullException>()
                    .WithMessage("*Value cannot be null*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task Put_WhenResponseIsUnsuccessful_ShouldThrowResponseCodeException()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicPut<object>(url, payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ResponseCodeException>()
                    .WithMessage("*Bad Request*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Put &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task Put_WhenUrlIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicPut<object>("", payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ArgumentNullException>()
                    .WithMessage("*Value cannot be null*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Put &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task PutNonGeneric_WhenResponseIsUnsuccessful_ShouldThrowResponseCodeException()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicPut(url, payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage("*Bad Request*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Put &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task Put_NonGenericWhenUrlIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicPut("", payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ArgumentNullException>()
                    .WithMessage("*Value cannot be null*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Put &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task Delete_WhenResponseIsUnsuccessful_ShouldThrowResponseCodeException()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicDelete<object>(url, payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ResponseCodeException>()
                    .WithMessage("*Bad Request*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Delete &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task Delete_WhenUrlIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicDelete<object>("", payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ArgumentNullException>()
                    .WithMessage("*Value cannot be null*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Delete &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task DeleteNonGeneric_WhenResponseIsUnsuccessful_ShouldThrowResponseCodeException()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicDelete(url, payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage("*Bad Request*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Delete &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task DeleteNonGeneric_WhenUrlIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var payload = new { Id = 1, Name = "Test" };
            var cancellationToken = CancellationToken.None;

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request"),
                })
                .Verifiable();

            // Act
            Func<Task> act = async () => await _testableHttpService.PublicDelete("", payload, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ArgumentNullException>()
                    .WithMessage("*Value cannot be null*");

                _handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Delete &&
                        msg.RequestUri!.ToString() == expectedUrl &&
                        msg.Content!.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public void SetBearerToken_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var token = "testToken";

            // Act
            _testableHttpService.PublicSetBearerToken(token);

            // Assert
            using (new AssertionScope())
            {
                _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
                _httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
                _httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(token);
            }
        }

        [TestMethod]
        public void Constructor_WhenHttpContextAccessorIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new TestableBaseHttpService(
                _httpClient,
                null!, // Pass null for IHttpContextAccessor
                baseUrl);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*httpContextAccessor*");
        }

        [TestMethod]
        public void Constructor_WhenHttpClientFactoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new TestableBaseHttpService(
                null!, // Pass null for HttpClient
                _httpContextAccessorMock.Object,
                baseUrl);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*httpClient*");
        }

        [TestMethod]
        public void Constructor_WhenBaseUrlIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient(); // Provide a valid HttpClient instance

            // Act
            Action act = () => new TestableBaseHttpService(
                httpClient,
                _httpContextAccessorMock.Object,
                null!); // Simulate null baseUrl

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*baseUrl*");
        }

        [TestMethod]
        public void Constructor_WhenEndPointNameIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new TestableBaseHttpService(
                new HttpClient(),
                _httpContextAccessorMock.Object,
                null!); // Simulate null for endPointName

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*baseUrl*"); // Verify that the correct parameter is reported
        }

        [TestMethod]
        public void Constructor_WhenTrailingSlash_ShouldTrimTrailingSlashFromBaseUrl()
        {
            // Arrange
            var trimmedBaseUrl = baseUrl.TrimEnd('/');
            var httpClient = new HttpClient();

            // Act
            var service = new TestableBaseHttpService(httpClient, _httpContextAccessorMock.Object, $"{baseUrl}/");

            // Assert
            service.BaseUrl.Should().Be(trimmedBaseUrl);
        }

        [TestMethod]
        public void Constructor_WhenNoTrailingSlash_ShouldNotTrim()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            var service = new TestableBaseHttpService(httpClient, _httpContextAccessorMock.Object, baseUrl);

            // Assert
            service.BaseUrl.Should().Be(baseUrl);
        }
    }
}
