using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    // TODO : PS - assumed there will be more values to add here
    public class PaymentStatusInsertRequestDto
    {
        public string? Status { get; set; } // TODO : PS - maybe change to Enum when values are known
    }
}
