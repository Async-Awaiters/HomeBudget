using HomeBudget.AuthService.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{
    public interface IUserRepository : IGetRepository<User>, IAddRepository<User>, IUpdateRepository<User>
    {
    }
}
