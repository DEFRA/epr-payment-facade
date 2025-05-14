using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter
{
    public class ReprocessorOrExporterRegistrationFeesResponseDto
    {
        public required string MaterialType { get; set; } // "Plastic", "Paper", "Metal", etc.

        public required decimal RegistrationFee { get; set; } // The registration fee for the material type.

        public PreviousPaymentDetailResponseDto? PreviousPaymentDetail { get; set; } // The previous payment detail for the re-processor/exporter.
    }
}
