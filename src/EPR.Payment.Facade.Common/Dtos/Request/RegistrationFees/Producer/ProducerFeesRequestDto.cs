using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;

namespace EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer
{
    public class ProducerFeesRequestDto  : ProducerFeesRequestseBaseDto
    {
        public Guid? FileId { get; set; }

        public bool IsProducerOnlineMarketplace { get; set; } // True or False

        public bool IsClosedLoopRecycling { get; set; } // True or False

        public bool IsLateFeeApplicable { get; set; } // True or False
    }
}