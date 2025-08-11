using HomeBudget.Directories.EF.DAL.Models;

namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface ICategoriesRepository : IGetByUserIdRepository<Category>, ICreateRepository<Category>, IUpdateByUserIdRepository<Category>, IDeleteByUserIdRepository<Category>, IDisposable
    {

    }
}
