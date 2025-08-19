using HomeBudget.Directories.Models.Categories.Requests;
using HomeBudget.Directories.ValidationHelpers.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HomeBudget.Directories.ValidationHelpers.Implementations
{
    public class CreateRequestValidator : IRequestValidator<CreateCategoryRequest>
    {
        public void Validate(CreateCategoryRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request), "Create request cannot be null.");

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
