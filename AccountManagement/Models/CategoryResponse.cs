namespace AccountManagement.Models;

public class CategoryResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }
}
