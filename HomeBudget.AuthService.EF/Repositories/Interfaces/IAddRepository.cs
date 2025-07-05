namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{
    public interface IAddRepository<TEntity>
    {
        Task AddUserAsync(TEntity entity, CancellationToken ct);
    }
}
