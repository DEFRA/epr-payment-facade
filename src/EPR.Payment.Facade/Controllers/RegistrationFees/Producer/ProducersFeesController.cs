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
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/producer")]
    [FeatureGate("EnableProducersFeesFeature")]
    public class ProducersFeesController : ControllerBase
    {
        private readonly IProducerFeesService _producerFeesService;
        private readonly ILogger<ProducersFeesController> _logger;
        private readonly IValidator<ProducerFeesRequestDto> _registrationValidator;
        private readonly IValidator<ProducerFeesRequestV2Dto> _registrationV2Validator;

        public ProducersFeesController(
            IProducerFeesService producerFeesService,
            ILogger<ProducersFeesController> logger,
            IValidator<ProducerFeesRequestDto> registrationValidator,
            IValidator<ProducerFeesRequestV2Dto> registrationV2Validator
            )
        {
            _producerFeesService = producerFeesService ?? throw new ArgumentNullException(nameof(producerFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registrationValidator = registrationValidator ?? throw new ArgumentNullException(nameof(registrationValidator));
            _registrationV2Validator = registrationV2Validator ?? throw new ArgumentNullException(nameof(registrationValidator));
        }

        [ApiExplorerSettings(GroupName = "v1")]
        [HttpPost("v1/registration-fee")]
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
        public async Task<IActionResult> CalculateFeesAsync([FromBody] ProducerFeesRequestDto producerRegistrationFeesRequestDto, CancellationToken cancellationToken)
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


        [ApiExplorerSettings(GroupName = "v2")]
        [MapToApiVersion(2)]
        [HttpPost("v2/registration-fee")]
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
        [FeatureGate("EnableProducersFeesCalculationV3")]
        public async Task<IActionResult> CalculateFeesAsync([FromBody] ProducerFeesRequestV2Dto producerRegistrationFeesRequestDto, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = _registrationV2Validator.Validate(producerRegistrationFeesRequestDto);
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
