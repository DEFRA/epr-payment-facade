﻿using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;

namespace EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer
{
    public class HttpProducerResubmissionFeesService : BaseHttpService, IHttpProducerResubmissionFeesService
    {
        public HttpProducerResubmissionFeesService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<Service> config)
            : base(httpClient,
                   httpContextAccessor,
                   config.Value.Url ?? throw new ArgumentNullException(nameof(config), ExceptionMessages.RegistrationFeesServiceBaseUrlMissing))
        {
            // Any additional setup if required
        }

        public async Task<ProducerResubmissionFeeResponseDto> GetResubmissionFeeAsync(
            ProducerResubmissionFeeRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Post<ProducerResubmissionFeeResponseDto>(UrlConstants.GetProducerResubmissionFee, request, cancellationToken);
            }
            catch (ResponseCodeException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ValidationException(ex.Message.Trim('"'));
            }
            catch (Exception ex)
            {
                throw new ServiceException(ExceptionMessages.ErrorResubmissionFees, ex);
            }
        }
    }
}
