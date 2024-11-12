namespace EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme
{
    public class ComplianceSchemeResubmissionFeeResponse
    {
        public decimal TotalResubmissionFee { get; set; }
        public decimal PreviousPayments { get; set; }
        public decimal OutstandingPayment { get; set; }
        public int MemberCount { get; set; }
    }
}