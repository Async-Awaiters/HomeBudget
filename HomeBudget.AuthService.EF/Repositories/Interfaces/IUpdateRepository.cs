using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{
    public interface IUpdateRepository<TEntity>
    {
        Task UpdateAsync(TEntity entity, CancellationToken ct);
    }
}
