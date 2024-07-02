using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class CompletePaymentRequestDto
    {
        [Required(ErrorMessage = "External Payment ID is required")]
        public Guid ExternalPaymentId { get; set; }

        [Required(ErrorMessage = "Updated By User ID is required")]
        public string? UpdatedByUserId { get; set; }

        [Required(ErrorMessage = "Updated By Organisation ID is required")]
        public string? UpdatedByOrganisationId { get; set; }
    }
}
