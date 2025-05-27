using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees
{
    public class AccreditationFeesResponseDto
    {
        public decimal? OverseasSiteChargePerSite { get; set; }

        public decimal? TotalOverseasSitesCharges { get; set; }

        public decimal? TonnageBandCharge { get; set; }

        public decimal? TotalAccreditationFees { get => TonnageBandCharge + TotalOverseasSitesCharges; }

        public AccreditationFeesPreviousPayment? PreviousPaymentDetail { get; set; }
    }
}
