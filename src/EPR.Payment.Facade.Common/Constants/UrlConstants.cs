﻿namespace EPR.Payment.Facade.Common.Constants
{
    public static class UrlConstants
    {
        // Payments service endpoints
        public const string PaymentsInsert = "payments";
        public const string PaymentsUpdate = "payments/{externalPaymentId}";
        public const string GetPaymentDetails = "payments/{externalPaymentId}";

        // GovPay service endpoints
        public const string GovPayInitiatePayment = "payments";
        public const string GovPayGetPaymentStatus = "payments/{paymentId}";
    }
}
