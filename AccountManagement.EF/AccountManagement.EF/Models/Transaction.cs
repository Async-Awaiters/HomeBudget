using System.ComponentModel.DataAnnotations.Schema;

namespace AccountManagement.EF.Models;

/// <summary>
/// Модель представления транзакции в системе управления учетными записями
/// </summary>
public class Transaction
{
    /// <summary>
    /// Уникальный идентификатор транзакции (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Дата фактического совершения транзакции
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Планируемая дата транзакции (опционально)
    /// </summary>
    public DateTime? PlanDate { get; set; }

    /// <summary>
    /// Идентификатор связанной учетной записи
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    /// Сумма транзакции в валюте учетной записи
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Описание транзакции (опционально)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Флаг подтверждения транзакции (опционально)
    /// </summary>
    public bool? IsApproved { get; set; }

    /// <summary>
    /// Флаг повторяемости транзакции (опционально)
    /// </summary>
    public bool? IsRepeated { get; set; }

    /// <summary>
    /// Флаг удаления транзакции из системы
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Связанная учетная запись, к которой относится транзакция
    /// </summary>
    public Account? Account { get; set; }
}
