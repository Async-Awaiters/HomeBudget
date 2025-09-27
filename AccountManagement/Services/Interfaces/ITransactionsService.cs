using AccountManagement.EF.Models;

namespace AccountManagement.Services.Interfaces;

public interface ITransactionsService : IService<Transaction>
{
    Task ConfirmAsync(Guid transactionId, Guid userId);
}
