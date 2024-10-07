using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EPR.Payment.Facade.Common.Validators
{
    public class ValidUrlAttribute : ValidationAttribute
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("The URL is required.");
            }

            string url = value.ToString()!;
            string pattern = @"^(http|https)://([\w-]+(\.[\w-]+)+)([/#?]?.*)$";

            try
            {
                return Regex.IsMatch(url, pattern, RegexOptions.None, RegexTimeout)
                    ? ValidationResult.Success!
                    : new ValidationResult("The URL is not valid.");
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationResult("The URL validation timed out.");
            }
        }
    }
}
