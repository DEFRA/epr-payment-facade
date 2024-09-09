﻿using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;

namespace EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces
{
    public interface IHttpRegistrationFeesService
    {
        Task<RegistrationFeesResponseDto> CalculateProducerFeesAsync(ProducerRegistrationFeesRequestDto request, CancellationToken cancellationToken = default);
        Task<decimal?> GetResubmissionFeeAsync(string regulator, CancellationToken cancellationToken = default);
    }
}