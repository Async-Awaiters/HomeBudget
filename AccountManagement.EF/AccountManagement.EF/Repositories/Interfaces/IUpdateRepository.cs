namespace AccountManagement.EF.Repositories.Interfaces;

public interface IUpdateRepository<TEntity>
{
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
}

