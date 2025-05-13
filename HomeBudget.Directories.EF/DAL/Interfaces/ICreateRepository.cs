using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface ICreateRepository<TEntity>
    {
        Task Create(TEntity entity);
    }
}
