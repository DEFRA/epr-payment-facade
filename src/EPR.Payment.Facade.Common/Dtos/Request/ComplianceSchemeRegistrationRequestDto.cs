using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos
{
    public class ComplianceSchemeRegistrationRequestDto
    {
        public int NumberOfLargeProducers { get; set; }
        public int NumberOfSmallProducers { get; set; }
        public int NumberOfOnlineMarketplaces { get; set; }

        [Range(0, 100, ErrorMessage = "The number of subsidiaries must be between 0 and 100.")]
        public int NumberOfSubsidiaries { get; set; }

        public bool PayBaseFeeAlone { get; set; }
    }
}
