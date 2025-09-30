using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Services.RegistrationFees.Producer.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.RegistrationFees.Producer
{
    [ApiVersion(3)]
    [ApiController]
    [Route("api/v{version:apiVersion}/producer")]
    [FeatureGate("EnableProducersFeesFeature")]
    public class ProducersFeesV3Controller : ControllerBase
    {
        private readonly IProducerFeesService _producerFeesService;
        private readonly ILogger<ProducersFeesV3Controller> _logger;
        private readonly IValidator<ProducerFeesRequestV3Dto> _registrationValidator;

        public ProducersFeesV3Controller(
            IProducerFeesService producerFeesService,
            ILogger<ProducersFeesV3Controller> logger,
            IValidator<ProducerFeesRequestV3Dto> registrationValidator
            )
        {
            _producerFeesService = producerFeesService ?? throw new ArgumentNullException(nameof(producerFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registrationValidator = registrationValidator ?? throw new ArgumentNullException(nameof(registrationValidator));
        }

        [ApiExplorerSettings(GroupName = "v3")]
        [MapToApiVersion(3)]
        [HttpPost("registration-fee")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProducerFeesResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Calculates the producer registration fees",
            Description = "Calculates the producer registration fees based on the provided request data."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the calculated fees for the producer.", typeof(ProducerFeesResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        [FeatureGate("EnableProducersFeesCalculation")]
        public async Task<IActionResult> CalculateFeesAsync([FromBody] ProducerFeesRequestV3Dto producerRegistrationFeesRequestDto, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = _registrationValidator.Validate(producerRegistrationFeesRequestDto);
            if (!validationResult.IsValid)
            {
                _logger.LogError(LogMessages.ValidationErrorOccured, nameof(CalculateFeesAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var result = await _producerFeesService.CalculateProducerFeesAsync(producerRegistrationFeesRequestDto, cancellationToken);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateFeesAsync));
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingProducerFees, nameof(CalculateFeesAsync));
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Service Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingProducerFees, nameof(CalculateFeesAsync));
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected Error",
                    Detail = ExceptionMessages.UnexpectedErrorCalculatingFees,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
