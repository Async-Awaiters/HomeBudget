namespace AccountManagement.Models;

public class ReportDataRow
{
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
}
