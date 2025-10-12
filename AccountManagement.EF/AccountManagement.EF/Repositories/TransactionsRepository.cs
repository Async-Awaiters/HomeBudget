using AccountManagement.EF.Exceptions;
using AccountManagement.EF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountManagement.EF.Repositories.Interfaces;

/// <summary>
/// Репозиторий для работы с транзакциями
/// </summary>
public class TransactionsRepository : ITransactionsRepository
{
    private readonly AccountManagementContext _context;
    // Используем выражения вместо строк для безопасного доступа к свойствам
    private static readonly Func<Transaction, object>[] UpdatableProperties =
    [
        t => t.Amount,
        t => t.Description!,
        t => t.Date
    ];
    private readonly ILogger<TransactionsRepository> _logger;

    public TransactionsRepository(AccountManagementContext context, ILogger<TransactionsRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Создает новую транзакцию
    /// </summary>
    public async Task CreateAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await ExecuteWithLogging(
            async () =>
            {
                ValidateTransaction(transaction);

                if (await _context.Transactions.AnyAsync(x => x.Id == transaction.Id, cancellationToken))
                    throw new EntityAlreadyExistsException("Транзакция с таким ID уже существует");

                await _context.Transactions.AddAsync(transaction, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Транзакция {Id} успешно создана", transaction.Id);
            },
            transaction.Id,
            "Ошибка при создании транзакции {Id}"
        );
    }

    /// <summary>
    /// Удаляет транзакцию по идентификатору
    /// </summary>
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID транзакции не может быть пустым", nameof(id));

        await ExecuteWithLogging(
            async () =>
            {
                var transaction = await GetByIdAsync(id, cancellationToken);
                if (transaction == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующую транзакцию {Id}", id);
                    return; // Безопасное удаление без исключения
                }

                await CheckAccountExists(transaction.AccountId, userId, cancellationToken);
                transaction.IsDeleted = true;
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Транзакция {Id} успешно удалена", id);
            },
            id,
            "Ошибка при удалении транзакции {Id}"
        );
    }

    /// <summary>
    /// Получает все транзакции для указанного счета
    /// </summary>
    public IQueryable<Transaction> GetAllAsync(Guid accountId, int pageNumber, int pageSize)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("ID счета не может быть пустым", nameof(accountId));

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        return _context.Transactions
            .Where(t => t.AccountId == accountId && !t.IsDeleted)
            .OrderByDescending(t => t.Date)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }

    /// <summary>
    /// Получает транзакцию по идентификатору
    /// </summary>
    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID транзакции не может быть пустым", nameof(id));

        try
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении транзакции {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующую транзакцию
    /// </summary>
    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        if (transaction.Id == Guid.Empty)
            throw new ArgumentException("ID транзакции не может быть пустым", nameof(transaction.Id));

        await ExecuteWithLogging(
            async () =>
            {
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(x => x.Id == transaction.Id, cancellationToken);

                if (existingTransaction == null)
                {
                    _logger.LogWarning("Попытка обновления несуществующей транзакции {Id}", transaction.Id);
                    return;
                }

                // Проверка, что accountId не изменился
                if (transaction.AccountId != existingTransaction.AccountId)
                {
                    throw new InvalidOperationException("Нельзя изменять accountId существующей транзакции");
                }

                // Обновление только допустимых свойств через выражения
                foreach (var property in UpdatableProperties)
                {
                    _context.Entry(existingTransaction).Property(property.Method.Name).CurrentValue = property(existingTransaction);
                }

                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Транзакция {Id} успешно обновлена", transaction.Id);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Конфликт параллелизма при обновлении транзакции {Id}. Попробуйте повторить запрос позже.", transaction.Id);
                    throw;
                }
            },
            transaction.Id,
            "Ошибка при обновлении транзакции {Id}"
        );
    }

    /// <summary>
    /// Выполняет операцию с логированием и обработкой исключений
    /// </summary>
    private async Task ExecuteWithLogging(Func<Task> operation, Guid id, string errorMessageFormat)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message} ({Id})", errorMessageFormat, id);
            throw;
        }
    }

    private void ValidateTransaction(Transaction transaction)
    {
        if (transaction.Id == Guid.Empty)
            throw new ArgumentException("ID транзакции не может быть пустым", nameof(transaction.Id));

        if (transaction.AccountId == Guid.Empty)
            throw new ArgumentException("ID аккаунта не может быть пустым", nameof(transaction.AccountId));

        if (string.IsNullOrWhiteSpace(transaction.Description) || transaction.Description.Length > 200)
            throw new ArgumentException("Описание транзакции должно быть не пустым и содержать не более 200 символов", nameof(transaction.Description));

        if (transaction.Amount <= 0)
            throw new ArgumentException("Сумма транзакции должна быть положительной", nameof(transaction.Amount));

        if (transaction.Date > DateTime.UtcNow)
            throw new ArgumentException("Дата транзакции не может быть в будущем", nameof(transaction.Date));
    }

    private async Task CheckAccountExists(Guid accountId, Guid userId, CancellationToken cancellationToken)
    {
        if (!await _context.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId && !a.IsDeleted, cancellationToken))
            throw new EntityNotFoundException("Счет не найден");
    }
}
