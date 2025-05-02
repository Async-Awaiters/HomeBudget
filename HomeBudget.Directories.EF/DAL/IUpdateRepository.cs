using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.Directories.EF.DAL
{
    public interface IUpdateRepository<TEntity> : IDisposable where TEntity : class
    {
        Task Update(TEntity entity);
    }
}
