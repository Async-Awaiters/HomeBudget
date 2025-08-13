using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace HomeBudget.Directories.EF.DAL.Models
{
    [Table("Categories")]
    public class Category
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? UserId { get; set; }
    }
}