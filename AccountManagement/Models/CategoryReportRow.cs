namespace AccountManagement.Models;

public class CategoryReportRow
{
    public Guid CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public decimal Amount { get; set; }
}
