using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees
{
    public class AccreditationFeesPreviousPayment
    {
        public string? PaymentMode { get; set; } // "offline" or "online"

        public string? PaymentMethod { get; set; }

        public decimal PaymentAmount { get; set; }

        public DateTime? PaymentDate { get; set; }
    }
}
