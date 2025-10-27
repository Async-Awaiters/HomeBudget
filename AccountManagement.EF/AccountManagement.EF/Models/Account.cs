using Newtonsoft.Json;

namespace AccountManagement.EF.Models;

/// <summary>
/// Модель данных банковского счёта.
/// </summary>
public class Account
{
    /// <summary>
    /// Уникальный идентификатор счёта.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Наименование счёта (например, "Основной счёт", "Кредитный счёт").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Идентификатор пользователя, которому принадлежит счёт.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Текущий баланс счёта в денежных единицах.
    /// Может быть отрицательным при превышении лимитов.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Лимит овердрафта (дополнительный лимит для отрицательного баланса).
    /// Значение null означает, что овердрафт запрещён.
    /// </summary>
    public decimal? OverdraftLimit { get; set; }

    /// <summary>
    /// Кредитный лимит для счёта.
    /// Значение null означает, что кредитный лимит не установлен.
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Тип счёта (например, текущий, депозитный, кредитный).
    /// </summary>
    public required AccountTypes Type { get; set; }

    /// <summary>
    /// Флаг активности счёта.
    /// По умолчанию установлен в true.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Флаг удаления счёта из системы.
    /// По умолчанию установлен в false.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Валюта, в которой хранится баланс счёта.
    /// </summary>
    public Guid CurrencyId { get; set; }

    /// <summary>
    /// Список транзакций, связанных с данным счётом.
    /// </summary>
    [JsonProperty("transactions", ReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public ICollection<Transaction> Transactions { get; } = new List<Transaction>(); // Транзакции связанные со счётом
}
