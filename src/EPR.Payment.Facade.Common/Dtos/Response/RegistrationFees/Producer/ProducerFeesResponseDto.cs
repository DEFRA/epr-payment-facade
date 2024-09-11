namespace EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer
{
    public class ProducerFeesResponseDto
    {
        public decimal? BaseFee { get; set; } // Nullable to handle scenarios where the fee is not applicable
        public decimal? SubsidiariesFee { get; set; } // Nullable to handle scenarios where the fee is not applicable
        public decimal TotalFee { get; set; }
        public List<FeeBreakdown> FeeBreakdowns { get; set; } = new();
    }

    public class FeeBreakdown
    {
        public required string Description { get; set; } = string.Empty; // Description of the fee component
        public decimal Amount { get; set; } // Fee amount, must be greater than 0
    }
}