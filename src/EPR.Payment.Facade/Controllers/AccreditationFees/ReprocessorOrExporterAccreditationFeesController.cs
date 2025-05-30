using Asp.Versioning;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Services.AccreditationFees.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace EPR.Payment.Facade.Controllers.AccreditationFees
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/reprocessorexporter")]
    [FeatureGate("EnableReprocessorOrExporterAccreditationFeesFeature")]
    public class ReprocessorOrExporterAccreditationFeesController(
       ILogger<ReprocessorOrExporterAccreditationFeesController> logger,
       IValidator<AccreditationFeesRequestDto> accreditationFeesRequestvalidator,
       IAccreditationFeesCalculatorService accreditationFeesCalculatorService) : ControllerBase
    {
        [HttpPost("accreditation-fee")]
        [ProducesResponseType(typeof(AccreditationFeesResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Calculates the accreditation fee for a exporter or reprocessor",
            Description = "Calculates the accreditation fee for a exporter or reprocessor based on provided request details."
        )]
        [FeatureGate("EnableReprocessorOrExporterAccreditationFeesCalculation")]
        public async Task<IActionResult> GetAccreditationFee([FromBody] AccreditationFeesRequestDto request,
            CancellationToken cancellationToken)
        {
            var validationResult = accreditationFeesRequestvalidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                AccreditationFeesResponseDto? accreditationFeesResponseDto = await accreditationFeesCalculatorService.CalculateAccreditationFeesAsync(request, cancellationToken);
           
                if (accreditationFeesResponseDto is null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Not Found Error",
                        Detail = "Accreditation fees data not found.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(accreditationFeesResponseDto);
            }
            catch (ValidationException ex)
            {
                logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(GetAccreditationFee));
                
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (ServiceException ex)
            {
                logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingReprocessorOrExporterAccreditationFees, nameof(GetAccreditationFee));

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Service Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, LogMessages.ErrorOccuredWhileCalculatingReprocessorOrExporterAccreditationFees, nameof(GetAccreditationFee));
                
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
