namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface IUpdateRepository<TEntity>
    {
        Task<bool> Update(TEntity entity, CancellationToken cancellationToken);
    }
}
