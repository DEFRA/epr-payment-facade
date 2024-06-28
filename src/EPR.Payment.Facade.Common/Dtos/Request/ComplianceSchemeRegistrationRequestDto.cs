using System.Collections.Generic;

namespace EPR.Payment.Facade.Common.Dtos
{
    public class ComplianceSchemeRegistrationRequestDto
    {
        public List<ProducerSubsidiaryInfo> Producers { get; set; } = new List<ProducerSubsidiaryInfo>();
        public bool PayComplianceSchemeBaseFee { get; set; }
    }
}
