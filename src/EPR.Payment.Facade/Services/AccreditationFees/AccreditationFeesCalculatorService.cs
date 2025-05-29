using System.ComponentModel.DataAnnotations;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.AccreditationFees.Interfaces;
using EPR.Payment.Facade.Services.AccreditationFees.Interfaces;

namespace EPR.Payment.Facade.Services.AccreditationFees
{
    public class AccreditationFeesCalculatorService(
        IHttpAccreditationFeesCalculatorService httpAccreditationFeesCalculatorService,
        ILogger<AccreditationFeesCalculatorService> logger) : IAccreditationFeesCalculatorService
    {
        public async Task<AccreditationFeesResponseDto> CalculateAccreditationFeesAsync(
            AccreditationFeesRequestDto accreditationFeesRequestDto,
            CancellationToken cancellationToken)
        {
            try
            {
                AccreditationFeesResponseDto accreditationFeesResponseDto = await httpAccreditationFeesCalculatorService.CalculateAccreditationFeesAsync(
                    accreditationFeesRequestDto,
                    cancellationToken);
                
                return accreditationFeesResponseDto;
            }
            catch (ValidationException ex)
            {
                logger.LogError(ex, LogMessages.ValidationErrorOccured, nameof(CalculateAccreditationFeesAsync));

                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ExceptionMessages.UnexpectedErrorCalculatingAccreditationFees);
                
                throw new ServiceException(ExceptionMessages.ErrorCalculatingAccreditationFees, ex);
            }
        }
    }
}
