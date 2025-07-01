namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface IUpdateRepository<TEntity>
    {
        Task Update(TEntity entity, CancellationToken cancellationToken);
    }
}
