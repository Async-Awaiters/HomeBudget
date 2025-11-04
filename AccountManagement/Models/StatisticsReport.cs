namespace AccountManagement.Models;

public class StatisticsReport
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }

    public decimal TotalAmount { get; set; }
    public List<CategoryReportRow> CategoryReport { get; set; } = new();
}
