namespace EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer
{
    public class ProducerResubmissionFeeResponseDto
    {
        public decimal TotalResubmissionFee { get; set; }
        public decimal PreviousPayments { get; set; }
        public decimal OutstandingPayment { get; set; }
    }
}