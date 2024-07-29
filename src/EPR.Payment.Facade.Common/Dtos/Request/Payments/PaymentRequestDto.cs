using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class PaymentRequestDto
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
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public int? Amount { get; set; }
    }
}
