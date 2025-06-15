using System.ComponentModel.DataAnnotations;

namespace HomeBudget.Directories.Services.DTO
{
    public class CreateCategoryDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public Guid? ParentId { get; set; }  // null для корневых категорий

        [Required]
        public Guid? UserId { get; set; }
    }
}
