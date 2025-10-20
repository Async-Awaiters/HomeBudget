namespace AccountManagement.EF.Repositories.Interfaces;

public interface IDeleteRepository<TEntity>
{
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
