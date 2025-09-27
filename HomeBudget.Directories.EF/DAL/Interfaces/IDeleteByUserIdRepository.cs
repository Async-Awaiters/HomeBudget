namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface IDeleteByUserIdRepository<TEntity> : IDisposable
    {
        Task Delete(Guid userId, Guid id, CancellationToken cancellationToken);
    }
}
