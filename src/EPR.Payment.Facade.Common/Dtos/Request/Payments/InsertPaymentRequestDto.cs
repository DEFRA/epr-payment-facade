using System.ComponentModel.DataAnnotations;
using EPR.Payment.Facade.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class InsertPaymentRequestDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Organisation ID is required")]
        public string? OrganisationId { get; set; }

        [Required(ErrorMessage = "Reference Number is required")]
        public string? ReferenceNumber { get; set; }

        [Required(ErrorMessage = "Regulator is required")]
        public string? Regulator { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public int? Amount { get; set; }

        [Required(ErrorMessage = "Reason For Payment is required")]
        public string? ReasonForPayment { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentStatus Status { get; set; }
    }
}
