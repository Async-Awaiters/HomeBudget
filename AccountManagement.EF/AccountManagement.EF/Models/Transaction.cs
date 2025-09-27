namespace AccountManagement.EF.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime? PlanDate { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public bool? IsApproved { get; set; }
    public bool? IsRepeated { get; set; }
    public bool IsDeleted { get; set; }

    public Account Account { get; set; } = null!;
}
