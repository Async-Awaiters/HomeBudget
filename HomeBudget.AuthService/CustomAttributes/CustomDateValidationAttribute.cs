using System.ComponentModel.DataAnnotations;

namespace HomeBudget.AuthService.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CustomDateValidationAttribute : ValidationAttribute
    {
        private readonly DateOnly _minDate = new DateOnly(1900, 1, 1); // Минимальная допустимая дата
        private readonly DateOnly _maxDate = DateOnly.FromDateTime(DateTime.UtcNow); // Максимум — сегодня

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success; // null допустим для DateOnly?

            if (value is DateOnly date)
            {
                if (date < _minDate)
                {
                    return new ValidationResult($"Birth date must be after {_minDate}.");
                }
                if (date > _maxDate)
                {
                    return new ValidationResult($"Birth date cannot be in the future.");
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid date format.");
        }
    }
}
