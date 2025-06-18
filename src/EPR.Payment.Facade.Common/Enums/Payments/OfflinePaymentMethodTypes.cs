using System.ComponentModel;

namespace EPR.Payment.Facade.Common.Enums.Payments
{
    public enum OfflinePaymentMethodTypes
    {
        [Description("Bank transfer")]
        BankTransfer = 1,

        [Description("Credit or debit card")]
        CreditOrDebitCard = 2,

        [Description("Cheque")]
        Cheque = 3,

        [Description("Cash")]
        Cash = 4
    }
}
