using HomeBudget.Directories.EF.DAL.Models;

namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface ICurrencyRepository : IGetRepository<Currency>, IDisposable
    {
    }
}
