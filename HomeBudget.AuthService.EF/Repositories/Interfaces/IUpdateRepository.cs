namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{
    public interface IUpdateRepository<TEntity>
    {
        Task UpdateUserAsync(TEntity entity, CancellationToken ct);
    }
}
