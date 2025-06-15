using HomeBudget.Directories.EF.DAL.Models;

namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface ICategoriesRepository : IGetRepository<Category>, ICreateRepository<Category>, IUpdateRepository<Category>, IDeleteRepository<Category>, IDisposable
    {

    }
}
