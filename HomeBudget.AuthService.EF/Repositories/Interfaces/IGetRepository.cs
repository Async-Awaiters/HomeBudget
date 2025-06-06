using HomeBudget.AuthService.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{ 
    public interface IGetRepository<TEntity>
    {
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<TEntity?> GetByLoginAsync(string login, CancellationToken ct);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    }
}
