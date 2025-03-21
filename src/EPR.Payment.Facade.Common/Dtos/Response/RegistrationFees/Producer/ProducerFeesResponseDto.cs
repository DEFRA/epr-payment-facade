﻿namespace EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer
{
    public class ProducerFeesResponseDto
    {
        public decimal ProducerRegistrationFee { get; set; } = 0; // Default to 0 if not applicable
        public decimal ProducerOnlineMarketPlaceFee { get; set; } = 0; // Default to 0 if not applicable
        public decimal ProducerLateRegistrationFee { get; set; } = 0; // Default to 0 if not applicable
        public decimal SubsidiariesFee { get; set; } = 0; // Default to 0 if not applicable
        public decimal TotalFee { get; set; } = 0; // Total fee will be computed
        public decimal PreviousPayment { get; set; }
        public decimal OutstandingPayment { get; set; }
        public required SubsidiariesFeeBreakdown SubsidiariesFeeBreakdown { get; set; }
    }
}