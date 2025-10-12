using AccountManagement.EF.Models;

namespace AccountManagement.TransactionProcessing;

/// <summary>
/// Интерфейс для обработки операций с транзакциями (добавление, удаление, обновление).
/// </summary>
public interface ITransactionProcessor
{
    /// <summary>
    /// Добавляет новую транзакцию к указанному счету.
    /// </summary>
    /// <param name="transaction">Объект транзакции, который нужно добавить.</param>
    /// <param name="account">Счет, к которому привязывается транзакция.</param>
    Task AddTransaction(Transaction transaction, Account account);

    /// <summary>
    /// Удаляет существующую транзакцию из указанного счета.
    /// </summary>
    /// <param name="transaction">Объект транзакции, который нужно удалить.</param>
    /// <param name="userId">Идентификатор пользователя, инициирующего удаление (для проверки прав).</param>
    /// <param name="account">Счет, из которого удаляется транзакция.</param>
    Task RemoveTransaction(Transaction transaction, Guid userId, Account account);

    /// <summary>
    /// Обновляет данные существующей транзакции на указанном счете.
    /// </summary>
    /// <param name="transaction">Объект транзакции с обновленными данными.</param>
    /// <param name="account">Счет, связанный с обновляемой транзакцией.</param>
    Task UpdateTransaction(Transaction transaction, Account account);
}
