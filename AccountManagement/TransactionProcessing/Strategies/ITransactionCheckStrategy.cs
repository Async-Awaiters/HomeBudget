using AccountManagement.EF.Models;

namespace AccountManagement.TransactionProcessing.Strategies;

/// <summary>
/// Интерфейс стратегий проверки транзакций.
/// </summary>
/// <remarks>
/// Предоставляет абстракцию для реализации различных правил проверки транзакций
/// в зависимости от типа аккаунта. Реализации должны определять логику проверки
/// перед выполнением операции.
/// </remarks>
public interface ITransactionCheckStrategy
{
    /// <summary>
    /// Тип аккаунта, для которого применяется данная стратегия.
    /// </summary>
    AccountTypes ForType { get; }

    /// <summary>
    /// Обрабатывает транзакцию согласно правилам стратегии.
    /// </summary>
    /// <param name="amount">Сумма транзакции.</param>
    /// <param name="account">Аккаунт, связанный с транзакцией.</param>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается при попытке выполнения недопустимой операции (например,
    /// отрицательная сумма для депозита).
    /// </exception>
    void ProcessTransaction(decimal amount, Account account);
}
