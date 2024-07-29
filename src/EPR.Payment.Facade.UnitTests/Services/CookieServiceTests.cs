using EPR.Payment.Facade.Common.Dtos.Internal.Payments;
using EPR.Payment.Facade.Services.Payments;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace EPR.Payment.Facade.UnitTests.Services
{
    [TestClass]
    public class CookieServiceTests
    {
        [TestMethod]
        public void SetPaymentDataCookie_SetsEncryptedCookie()
        {
            // Arrange
            var dataProtectionProvider = new EphemeralDataProtectionProvider();
            var cookieService = new CookieService(dataProtectionProvider);
            var httpContext = new DefaultHttpContext();
            var paymentData = new PaymentCookieDataDto
            {
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid(),
                GovPayPaymentId = "govPayPaymentId"
            };

            // Act
            cookieService.SetPaymentDataCookie(httpContext.Response, paymentData);

            // Assert
            var cookies = httpContext.Response.Headers["Set-Cookie"].ToString();
            cookies.Should().Contain("PaymentData=");
        }
    }
}