namespace AccountManagement.EF.Repositories.Interfaces;

public interface IGetRepository<TEntity>
{
    IQueryable<TEntity> GetAllAsync(Guid id, int pageNumber = 1, int pageSize = 100);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
