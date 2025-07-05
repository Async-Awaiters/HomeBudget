using HomeBudget.AuthService.EF.Models;

namespace HomeBudget.AuthService.EF.Repositories.Interfaces
{
    public interface IUserRepository : IGetRepository<User>, IAddRepository<User>, IUpdateRepository<User>
    {
    }
}
