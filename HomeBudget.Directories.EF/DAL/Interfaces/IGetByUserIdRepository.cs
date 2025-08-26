namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface IGetByUserIdRepository<TEntity>
    {
        IQueryable<TEntity> GetAll(Guid userId, CancellationToken cancellationToken);
        Task<TEntity?> GetById(Guid userId, Guid id, CancellationToken cancellationToken);
    }
}
