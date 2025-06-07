namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface ICreateRepository<TEntity>
    {
        Task Create(TEntity entity, CancellationToken cancellationToken);
    }
}
