namespace HomeBudget.Directories.EF.DAL.Models
{
    public class Categories
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? UserId { get; set; }

    }
}
