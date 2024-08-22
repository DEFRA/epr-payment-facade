using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EPR.Payment.Facade.Controllers.RegistrationFees
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/producers-fees")]
    [FeatureGate("EnableProducersFeesFeature")]
    public class ProducersFeesController : ControllerBase
    {
        private readonly IHttpRegistrationFeesService _registrationFeesService;
        private readonly ILogger<ProducersFeesController> _logger;
        private readonly IValidator<ProducerRegistrationFeesRequestDto> _validator;

        public ProducersFeesController(
            IHttpRegistrationFeesService registrationFeesService,
            ILogger<ProducersFeesController> logger,
            IValidator<ProducerRegistrationFeesRequestDto> validator)
        {
            _registrationFeesService = registrationFeesService ?? throw new ArgumentNullException(nameof(registrationFeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        [HttpPost("calculate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegistrationFeesResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Calculates the producer registration fees",
            Description = "Calculates the producer registration fees based on the provided request data."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the calculated fees for the producer.", typeof(RegistrationFeesResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "If the request is invalid or a validation error occurs.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "If an unexpected error occurs.", typeof(ProblemDetails))]
        [FeatureGate("EnableProducersFeesCalculation")]
        public async Task<IActionResult> CalculateFeesAsync([FromBody] ProducerRegistrationFeesRequestDto request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = _validator.Validate(request);
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
                var result = await _registrationFeesService.CalculateProducerFeesAsync(request, cancellationToken);
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
