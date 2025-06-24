namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{
    public interface IAddRepository<TEntity>
    {
        Task AddAsync(TEntity entity, CancellationToken ct);
    }
}
