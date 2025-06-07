using HomeBudget.Directories.EF.DAL.Models;

namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface ICategoriesRepository : IGetRepository<Categories>, ICreateRepository<Categories>, IUpdateRepository<Categories>, IDeleteRepository<Categories>, IDisposable
    {

    }
}
