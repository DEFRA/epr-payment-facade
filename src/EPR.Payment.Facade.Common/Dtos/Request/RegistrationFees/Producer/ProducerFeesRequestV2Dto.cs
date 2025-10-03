namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer
{
    public class ProducerFeesRequestV2Dto : ProducerFeesRequestseBaseDto
    {      
        public required Guid FileId { get; set; }

        public required int PayerId { get; set; }

        public required Guid ExternalId { get; set; }

        public required DateTimeOffset InvoicePeriod { get; set; }

        public required int PayerTypeId { get; set; }
    }
}