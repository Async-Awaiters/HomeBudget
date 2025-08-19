using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.ValidationHelpers.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HomeBudget.AuthService.ValidationHelpers
{
    public class LoginRequestValidator : IRequestValidator<LoginRequest>
    {
        public void Validate(LoginRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request), "Login request cannot be null.");

            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }
        }
    }
}