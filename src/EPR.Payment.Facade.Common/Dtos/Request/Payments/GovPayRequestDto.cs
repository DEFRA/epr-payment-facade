using EPR.Payment.Facade.Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class GovPayRequestDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid? UserId { get; set; }

        [Required(ErrorMessage = "Organisation ID is required")]
        public Guid? OrganisationId { get; set; }

        [Required(ErrorMessage = "Reference is required")]
        public string? Reference { get; set; }

        [Required(ErrorMessage = "Regulator is required")]
        public string? Regulator { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public int? Amount { get; set; }

        [Required(ErrorMessage = "Return URL is required")]
        [ValidUrl] // Custom attribute
        public string? return_url { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }
    }
}
