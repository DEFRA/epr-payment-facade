using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.Dtos.Response.AcceridiationFees.ReprocessorOrExporter
{
    public class ReprocessorOrExporterAcceridiationFeesResponseDto
    {
        public required decimal OverseasChargePerSite { get; set; } // The overseas charge per site for the exporter.

        public required decimal TotalOverseasSitesCharges { get; set; } // The total overseas sites charges for the exporter.

        public required decimal TonnageBandCharge { get; set; } // The tonnage band chanrge for the material type.

        public required decimal TotalAcceridiationFees { get; set; } // The total acceridiation fees for the re-processor/exporter.

        public PreviousPaymentDetailResponseDto? PreviousPaymentDetail { get; set; } // The previous payment detail for the re-processor/exporter.
    }
}
