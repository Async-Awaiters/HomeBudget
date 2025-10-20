using AccountManagement.EF.Models;

namespace AccountManagement.EF.Repositories.Interfaces;

public interface IAccountRepository : IGetRepository<Account>, ICreateRepository<Account>, IUpdateRepository<Account>, IDeleteRepository<Account>
{
}
