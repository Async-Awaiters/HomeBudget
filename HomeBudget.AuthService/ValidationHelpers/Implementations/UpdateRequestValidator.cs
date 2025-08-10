using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.ValidationHelpers.Interfaces;

namespace HomeBudget.AuthService.ValidationHelpers.Implementations
{
    public class UpdateRequestValidator : IRequestValidator<UpdateRequest>
    {
        public Dictionary<string, object?> ValidateRequest(UpdateRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Update request cannot be null.");
            }

            var validFields = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(request.Email))
                validFields["Email"] = request.Email;
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                validFields["FirstName"] = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.LastName))
                validFields["LastName"] = request.LastName;
            if (request.BirthDate.HasValue)
                validFields["BirthDate"] = request.BirthDate;

            return validFields;
        }
    }
}
