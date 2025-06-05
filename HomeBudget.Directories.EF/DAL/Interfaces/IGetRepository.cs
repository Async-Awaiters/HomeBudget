namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface IGetRepository<TEntity>
    {
        IQueryable<TEntity> GetAll(CancellationToken cancellationToken);
        Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken);
    }
}
