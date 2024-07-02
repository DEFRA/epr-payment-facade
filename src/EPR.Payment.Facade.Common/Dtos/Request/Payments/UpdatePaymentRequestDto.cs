using EPR.Payment.Facade.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class UpdatePaymentRequestDto
    {
        [Required(ErrorMessage = "External Payment ID is required")]
        public Guid ExternalPaymentId { get; set; }

        [Required(ErrorMessage = "GovPay Payment ID is required")]
        public string? GovPayPaymentId { get; set; }

        [Required(ErrorMessage = "Updated By User ID is required")]
        public string? UpdatedByUserId { get; set; }

        [Required(ErrorMessage = "Updated By Organisation ID is required")]
        public string? UpdatedByOrganisationId { get; set; }

        [Required(ErrorMessage = "Reference Number is required")]
        public string? ReferenceNumber { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentStatus Status { get; set; }

        public string? ErrorCode { get; set; }
    }
}
