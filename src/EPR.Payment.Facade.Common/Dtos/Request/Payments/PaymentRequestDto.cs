﻿using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.Common.Dtos.Request.Payments
{
    public class PaymentRequestDto
    {
        public Guid? UserId { get; set; }

        public Guid? OrganisationId { get; set; }

        public string? Reference { get; set; }

        public string? Regulator { get; set; }

        public int? Amount { get; set; }
    }
}
