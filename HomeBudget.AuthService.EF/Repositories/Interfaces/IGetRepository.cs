using HomeBudget.AuthService.EF.Models;

namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{ 
    public interface IGetRepository<TEntity>
    {
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<TEntity?> GetByLoginAsync(string login, CancellationToken ct);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    }
}
