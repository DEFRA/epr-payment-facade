using EPR.Payment.Facade.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class InsertOfflinePaymentRequestDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Reference is required")]
        public string? Reference { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public int? Amount { get; set; }

        public string? Description { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? Comments { get; set; }
    }
}