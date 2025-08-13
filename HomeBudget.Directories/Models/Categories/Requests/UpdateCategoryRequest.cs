using System.ComponentModel.DataAnnotations;

namespace HomeBudget.Directories.Models.Categories.Requests
{
    public class UpdateCategoryRequest : IValidatableObject
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string? Name { get; set; }

        public Guid? ParentId { get; set; }

        // Кастомная валидация — нужно хотя бы одно поле
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (String.IsNullOrWhiteSpace(Name) && !ParentId.HasValue)
            {
                yield return new ValidationResult(
                    "Необходимо заполнить хотя бы одно поле для обновления",
                    new[] { nameof(Name), nameof(ParentId) });
            }
        }
    }
}
