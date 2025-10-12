namespace AccountManagement.EF.Repositories.Interfaces;

public interface ICreateRepository<TEntity>
{
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken);
}
