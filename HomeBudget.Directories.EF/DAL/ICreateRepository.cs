using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.Directories.EF.DAL
{
    public interface ICreateRepository<TEntity> : IDisposable where TEntity : class
    {
        Task Create(TEntity entity);
    }
}
