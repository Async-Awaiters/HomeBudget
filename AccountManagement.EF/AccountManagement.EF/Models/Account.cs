namespace AccountManagement.EF.Models;

public class Account
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public decimal? OverdraftLimit { get; set; }
    public required AccountTypes AccountType { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public List<Transaction> Transactions { get; set; } = []; // Транзакции связанные со счётом
}
