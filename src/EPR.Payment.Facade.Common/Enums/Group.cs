using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.Enums
{
    public enum Group
    {
        [Description("Producer Type")]
        ProducerType = 1,

        [Description("Compliance Scheme")]
        ComplianceScheme = 2,

        [Description("Producer Subsidiaries")]
        ProducerSubsidiaries = 3,

        [Description("Compliance Scheme Subsidiaries")]
        ComplianceSchemeSubsidiaries = 4,

        [Description("Producer re-submitting a report")]
        ProducerResubmission = 5,

        [Description("Compliance Scheme re-submitting a report")]
        ComplianceSchemeResubmission = 6,

        [Description("Exporters")]
        Exporters = 7,

        [Description("Reprocessors")]
        Reprocessors = 8,
    }
}
