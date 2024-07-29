using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Internal.Payments
{
    public class PaymentCookieDataDto
    {
        [Required(ErrorMessage = "ExternalPaymentId is required")]
        public Guid ExternalPaymentId { get; set; }

        [Required(ErrorMessage = "Updated By User ID is required")]
        public Guid UpdatedByUserId { get; set; }

        [Required(ErrorMessage = "Updated By Organisation ID is required")]
        public Guid UpdatedByOrganisationId { get; set; }

        [Required(ErrorMessage = "GovPayPaymentId is required")]
        public string GovPayPaymentId { get; set; }
    }
}
