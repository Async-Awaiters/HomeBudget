using System.Text.Json.Serialization;

namespace AccountManagement.EF.Models;

/// <summary>
/// Перечисление типов учетных записей
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))] // Используется для сериализации в JSON через JsonStringEnumConverter
public enum AccountTypes
{
    /// <summary>
    /// Наличные деньги
    /// </summary>
    Cash,

    /// <summary>
    /// Дебетовая карта
    /// </summary>
    DebitCard,

    /// <summary>
    /// Кредитная карта
    /// </summary>
    CreditCard,

    /// <summary>
    /// Сберегательный счет
    /// </summary>
    SavingAccount,

    /// <summary>
    /// Полученный долг
    /// </summary>
    TakenDebt,

    /// <summary>
    /// Выданный кредит
    /// </summary>
    IssuedLoan,

    /// <summary>
    /// Другой тип (уточняется отдельно)
    /// </summary>
    Other
}
