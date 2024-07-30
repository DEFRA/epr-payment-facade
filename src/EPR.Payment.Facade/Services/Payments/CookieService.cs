using EPR.Payment.Facade.Common.Dtos.Internal.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System.Text;

namespace EPR.Payment.Facade.Services.Payments
{
    public class CookieService : ICookieService
    {
        private readonly IDataProtector _dataProtector;

        public CookieService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("PaymentDataProtector");
        }

        public void SetPaymentDataCookie(HttpResponse response, PaymentCookieDataDto paymentData)
        {
            var paymentDataJson = JsonConvert.SerializeObject(paymentData);
            var encryptedPaymentData = _dataProtector.Protect(Encoding.UTF8.GetBytes(paymentDataJson));
            var base64EncodedData = Convert.ToBase64String(encryptedPaymentData);

            response.Cookies.Append("PaymentData", base64EncodedData, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }

        public string RetrievePaymentDataCookie(string encryptedPaymentData)
        {
            var base64DecodedData = Convert.FromBase64String(encryptedPaymentData);
            var decryptedPaymentData = _dataProtector.Unprotect(base64DecodedData);
            return Encoding.UTF8.GetString(decryptedPaymentData);
        }
    }
}
