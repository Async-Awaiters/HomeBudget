using AccountManagement.EF.Models;

namespace AccountManagement.EF.Repositories.Interfaces;

public interface ITransactionsRepository : IGetRepository<Transaction>, ICreateRepository<Transaction>, IUpdateRepository<Transaction>, IDeleteRepository<Transaction>
{
}
