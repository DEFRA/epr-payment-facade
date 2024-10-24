using EPR.Payment.Facade.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class UpdateOnlinePaymentRequestDto
    {
        [Required(ErrorMessage = "ID is required")]
        public Guid ExternalPaymentId { get; set; }

        [Required(ErrorMessage = "GovPay Payment ID is required")]
        public string? GovPayPaymentId { get; set; }

        [Required(ErrorMessage = "Updated By User ID is required")]
        public Guid UpdatedByUserId { get; set; }

        [Required(ErrorMessage = "Updated By Organisation ID is required")]
        public Guid UpdatedByOrganisationId { get; set; }

        [Required(ErrorMessage = "Reference is required")]
        public string? Reference { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentStatus Status { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}