using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.Directories.Services.DTO
{
    public class CreateCategoryDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public Guid? ParentId { get; set; }  // null для корневых категорий

        [Required]
        public Guid UserId { get; set; }
    }
}
