using AccountManagement.Models;

namespace AccountManagement.ReportBuilder;

public interface IReportBuilder
{
    Task<StatisticsReport> Build(DateTime from, DateTime to, string token);
}
