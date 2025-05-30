using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees
{
    public class ReprocessorOrExporterAccreditationFeesResponseDto
    {
        public decimal? OverseasSiteChargePerSite { get; set; }

        public decimal? TotalOverseasSitesCharges { get; set; }

        public decimal? TonnageBandCharge { get; set; }

        public decimal? TotalAccreditationFees { get; set; }

        public PreviousPaymentDetailResponseDto? PreviousPaymentDetail { get; set; }
    }
}
