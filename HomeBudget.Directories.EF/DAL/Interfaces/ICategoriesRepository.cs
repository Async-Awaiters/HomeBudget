using HomeBudget.Directories.EF.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface ICategoriesRepository : IGetRepository<Categories>, ICreateRepository<Categories>, IUpdateRepository<Categories>, IDeleteRepository<Categories>, IDisposable
    {

    }
}
