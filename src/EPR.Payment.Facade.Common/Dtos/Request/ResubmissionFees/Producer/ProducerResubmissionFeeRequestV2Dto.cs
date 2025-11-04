namespace EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer
{
    public class ProducerResubmissionFeeRequestV2Dto : ProducerResubmissionFeeRequestDto
    {
        public required Guid FileId { get; set; }
        public required Guid ExternalId { get; set; }
        public required DateTimeOffset InvoicePeriod { get; set; }
        public required int PayerTypeId { get; set; }
        public required int PayerId { get; set; }
    }
}