using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.Directories.EF.DAL.Interfaces
{
    public interface ICurrencyRepository: IGetRepository<Currency>, ICreateRepository<Currency>, IUpdateRepository<Currency>, IDisposable
    {
        
    }
}
