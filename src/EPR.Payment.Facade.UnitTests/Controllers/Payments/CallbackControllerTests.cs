using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Internal.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using EPR.Payment.Facade.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Text;

namespace EPR.Payment.Facade.UnitTests.Controllers
{
    [TestClass]
    public class CallbackControllerTests
    {
        private IFixture? _fixture;

        [TestInitialize]
        public void Initialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
        }

        [TestMethod, AutoMoqData]
        public void RetrievePaymentData_CookieNotFound_ReturnsBadRequest(
            [Frozen] Mock<ICookieService> cookieServiceMock,
            [Frozen] Mock<ILogger<CallbackController>> loggerMock,
            CallbackController controller)
        {
            // Arrange
            var context = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = controller.RetrievePaymentData() as BadRequestObjectResult;

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull();
                result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
                var problemDetails = result.Value as ProblemDetails;
                problemDetails!.Title.Should().Be("Payment Data Not Found");
            }
        }

        [TestMethod, AutoMoqData]
        public void RetrievePaymentData_ValidCookie_ReturnsOk(
            [Frozen] Mock<ICookieService> cookieServiceMock,
            [Frozen] Mock<ILogger<CallbackController>> loggerMock,
            CallbackController controller,
            PaymentCookieDataDto paymentData)
        {
            // Arrange
            var paymentDataJson = JsonConvert.SerializeObject(paymentData);
            var encryptedPaymentData = Convert.ToBase64String(Encoding.UTF8.GetBytes(paymentDataJson));

            cookieServiceMock.Setup(s => s.RetrievePaymentDataCookie(encryptedPaymentData)).Returns(paymentDataJson);

            var context = new DefaultHttpContext();
            context.Request.Headers["Cookie"] = $"PaymentData={encryptedPaymentData}";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = controller.RetrievePaymentData() as OkObjectResult;

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull();
                result!.StatusCode.Should().Be(StatusCodes.Status200OK);
                var resultData = result.Value as PaymentCookieDataDto;
                resultData.Should().BeEquivalentTo(paymentData);
            }
        }

        [TestMethod, AutoMoqData]
        public void RetrievePaymentData_ExceptionThrown_ReturnsInternalServerError(
            [Frozen] Mock<ICookieService> cookieServiceMock,
            [Frozen] Mock<ILogger<CallbackController>> loggerMock,
            CallbackController controller)
        {
            // Arrange
            var encryptedPaymentData = "invalidData";
            cookieServiceMock.Setup(s => s.RetrievePaymentDataCookie(encryptedPaymentData)).Throws(new Exception("Decryption error"));

            var context = new DefaultHttpContext();
            context.Request.Headers["Cookie"] = $"PaymentData={encryptedPaymentData}";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = controller.RetrievePaymentData() as ObjectResult;

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull();
                result!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
                var problemDetails = result.Value as ProblemDetails;
                problemDetails!.Title.Should().Be("Internal Server Error");
            }
        }
    }
}
