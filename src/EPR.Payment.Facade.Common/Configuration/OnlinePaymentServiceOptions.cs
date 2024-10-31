using EPR.Payment.Facade.Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Configuration
{
    public class OnlinePaymentServiceOptions
    {
        [Required(ErrorMessage = "Return URL is required")]
        [ValidUrl]
        public string? ReturnUrl { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Error URL is required")]
        [ValidUrl]
        public string? ErrorUrl { get; set; }
    }
}
