using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace HomeBudget.Directories.EF.DAL.Models
{
    [Table("Categories")]
    public class Category
    {
        [SwaggerIgnore]
        public Guid Id { get; set; }

        [JsonRequired]
        public required string Name { get; set; }

        [JsonRequired]
        public bool IsDeleted { get; set; }

        public Guid? ParentId { get; set; }

        public Guid? UserId { get; set; }
    }
}