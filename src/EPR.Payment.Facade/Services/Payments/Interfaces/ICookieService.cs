using EPR.Payment.Facade.Common.Dtos.Internal.Payments;

namespace EPR.Payment.Facade.Services.Payments.Interfaces
{
    public interface ICookieService
    {
        void SetPaymentDataCookie(HttpResponse response, PaymentCookieDataDto paymentData);
        string RetrievePaymentDataCookie(string encryptedPaymentData);
    }
}
