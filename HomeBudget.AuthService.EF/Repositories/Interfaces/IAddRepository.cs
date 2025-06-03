using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{
    public interface IAddRepository<TEntity>
    {
        Task AddAsync(TEntity entity, CancellationToken ct);
    }
}
