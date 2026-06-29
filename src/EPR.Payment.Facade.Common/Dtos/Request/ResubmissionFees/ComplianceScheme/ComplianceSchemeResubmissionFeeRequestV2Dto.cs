namespace EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme
{
    public class ComplianceSchemeResubmissionFeeRequestV2Dto : ComplianceSchemeResubmissionFeeRequestDto
    {
        public new required Guid FileId { get; set; }
        public required Guid ExternalId { get; set; }
        public required DateTimeOffset InvoicePeriod { get; set; }
        public required int PayerTypeId { get; set; }
        public required int PayerId { get; set; }
    }
}