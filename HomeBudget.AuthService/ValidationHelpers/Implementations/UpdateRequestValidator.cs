using HomeBudget.AuthService.EF.Models;
using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.ValidationHelpers.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HomeBudget.AuthService.ValidationHelpers.Implementations
{
    public class UpdateRequestValidator : IUpdateRequestValidator<UpdateRequest>
    {
        public Dictionary<string, object?> Validate(UpdateRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Update request cannot be null.");
            }

            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            var validFields = new Dictionary<string, object?>
            {
                { nameof(User.Email), request.Email },
                { nameof(User.FirstName), request.FirstName },
                { nameof(User.LastName), request.LastName },
                { nameof(User.BirthDate), request.BirthDate }
            }
            .Where(kvp => kvp.Value != null && (kvp.Key == nameof(User.BirthDate) || !string.IsNullOrWhiteSpace(kvp.Value?.ToString())))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (!validFields.Any())
            {
                throw new ArgumentException("Update request cannot be empty or contain only empty strings. At least one field must be provided with a valid value.", nameof(request));
            }

            return validFields;
        }
    }
}
