using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HomeBudget.Directories.Models.Categories.Requests
{
    public class CreateCategoryRequest
    {
        [JsonRequired]
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public required string Name { get; set; } = default!;

        public Guid? ParentId { get; set; }
    }
}
