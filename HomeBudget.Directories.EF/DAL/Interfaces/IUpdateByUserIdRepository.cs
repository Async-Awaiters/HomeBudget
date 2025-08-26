namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface IUpdateByUserIdRepository<TEntity>
    {
        Task Update(Guid userId, Guid id, TEntity entity, CancellationToken cancellationToken);
    }
}
