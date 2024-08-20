namespace EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees
{
    public class RegistrationFeeResponseDto
    {
        public decimal? BaseFee { get; set; }
        public decimal? SubsidiariesFee { get; set; }
        public decimal TotalFee { get; set; }
        public List<FeeBreakdown> FeeBreakdowns { get; set; } = new List<FeeBreakdown>();
    }

    public class FeeBreakdown
    {
        public required string Description { get; set; }
        public decimal Amount { get; set; }
    }
}
