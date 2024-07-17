﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class CompletePaymentRequestDto
    {
        [Required(ErrorMessage = "ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Updated By User ID is required")]
        public Guid UpdatedByUserId { get; set; }

        [Required(ErrorMessage = "Updated By Organisation ID is required")]
        public Guid UpdatedByOrganisationId { get; set; }
    }
}
