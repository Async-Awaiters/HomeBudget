
namespace HomeBudget.Directories.EF.DAL
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        IEnumerable<TEntity> GetAll();
        TEntity GetById(Guid id);
        void Create(TEntity entity);
        void Delete(Guid id);
        void Update(TEntity entity);
        void Save();
    }
}
