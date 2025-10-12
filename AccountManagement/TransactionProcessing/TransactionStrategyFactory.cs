using AccountManagement.EF.Models;
using AccountManagement.TransactionProcessing.Strategies;

namespace AccountManagement.TransactionProcessing;

/// <summary>
/// Фабрика для получения стратегий проверки транзакций в зависимости от типа аккаунта
/// </summary>
/// <remarks>
/// Реализует паттерн "Стратегия" для выбора подходящей проверки транзакции
/// </remarks>
public class TransactionCheckStrategyFactory : ITransactionStrategyFactory
{
    /// <summary>
    /// Словарь сопоставляющий типы аккаунтов соответствующим стратегиям проверки
    /// </summary>
    private readonly Dictionary<AccountTypes, ITransactionCheckStrategy> _strategies;

    /// <summary>
    /// Инициализирует новый экземпляр фабрики
    /// </summary>
    /// <param name="strategies">Список доступных стратегий проверки транзакций</param>
    public TransactionCheckStrategyFactory(IEnumerable<ITransactionCheckStrategy> strategies)
    {
        // Создаем словарь для быстрого поиска стратегии по типу аккаунта
        _strategies = strategies.ToDictionary(s => s.ForType, s => s);
    }

    /// <summary>
    /// Получает стратегию проверки транзакции для указанного типа аккаунта
    /// </summary>
    /// <param name="type">Тип аккаунта для которого требуется стратегия</param>
    /// <returns>Экземпляр стратегии проверки транзакции</returns>
    /// <exception cref="ArgumentException">Выбрасывается если для указанного типа нет зарегистрированной стратегии</exception>
    public ITransactionCheckStrategy GetStrategy(AccountTypes type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
        {
            return strategy;
        }
        throw new ArgumentException($"Не найдена стратегия для типа: {type}");
    }
}
