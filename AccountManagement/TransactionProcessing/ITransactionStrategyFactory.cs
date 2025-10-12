using AccountManagement.EF.Models;
using AccountManagement.TransactionProcessing.Strategies;

namespace AccountManagement.TransactionProcessing;

/// <summary>
/// Фабричный интерфейс для создания стратегий обработки транзакций в зависимости от типа аккаунта
/// </summary>
/// <remarks>
/// Используется для получения соответствующей стратегии проверки транзакций
/// в системе управления учетными записями
/// </remarks>
public interface ITransactionStrategyFactory
{
    /// <summary>
    /// Получает стратегию проверки транзакции для указанного типа аккаунта
    /// </summary>
    /// <param name="type">Тип аккаунта, для которого требуется стратегия</param>
    /// <returns>Экземпляр стратегии проверки транзакций, соответствующей указанному типу аккаунта</returns>
    ITransactionCheckStrategy GetStrategy(AccountTypes type);
}
