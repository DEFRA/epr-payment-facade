using EPR.Payment.Facade.Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Configuration
{
    public class OfflinePaymentServiceOptions
    {
        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }
    }
}
