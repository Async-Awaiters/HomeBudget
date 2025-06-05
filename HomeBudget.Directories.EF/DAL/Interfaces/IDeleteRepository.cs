namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface IDeleteRepository<TEntity> : IDisposable
    {
        Task Delete(Guid id, CancellationToken cancellationToken);
    }
}
