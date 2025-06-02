using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface IGetRepository<TEntity>
    {
        Task<IQueryable<TEntity>> GetAll(CancellationToken cancellationToken);
        Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken);
    }
}
