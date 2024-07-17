using EPR.Payment.Facade.Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Configuration
{
    public class PaymentServiceOptions
    {
        [Required(ErrorMessage = "Return URL is required")]
        [ValidUrl]
        public string? ReturnUrl { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }
    }
}
