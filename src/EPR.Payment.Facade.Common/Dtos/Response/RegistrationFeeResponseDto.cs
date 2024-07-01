namespace EPR.Payment.Facade.Common.Dtos
{
    public class RegistrationFeeResponseDto
    {
        /// <summary>
        /// The base fee calculated for the registration.
        /// </summary>
        public decimal? BaseFee { get; set; }

        /// <summary>
        /// The total fee calculated for all subsidiaries.
        /// </summary>
        public decimal? SubsidiariesFee { get; set; }

        /// <summary>
        /// The total fee calculated for all producers.
        /// </summary>
        public decimal? ProducersFee { get; set; }

        /// <summary>
        /// The total calculated registration fee.
        /// </summary>
        public decimal TotalFee { get; set; }

        /// <summary>
        /// Detailed breakdown of the fees, if applicable.
        /// </summary>
        public List<FeeBreakdown> FeeBreakdowns { get; set; } = new List<FeeBreakdown>();
    }

    public class FeeBreakdown
    {
        /// <summary>
        /// Description of the fee component (e.g., base fee, subsidiary fee).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Amount for this component of the fee.
        /// </summary>
        public decimal? Amount { get; set; }
    }
}
