using AccountManagement.EF.Exceptions;
using AccountManagement.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.EF.Repositories.Interfaces;

/// <summary>
/// Репозиторий для работы с сущностью Account в базе данных.
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly AccountManagementContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AccountRepository"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="context"/> равен null.</exception>
    public AccountRepository(AccountManagementContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Асинхронно создает новый аккаунт в базе данных.
    /// Проверяет уникальность имени аккаунта перед сохранением.
    /// </summary>
    /// <param name="account">Объект аккаунта для создания.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="account"/> равен null.</exception>
    /// <exception cref="EntityAlreadyExistsException">Выбрасывается, если аккаунт с таким именем уже существует.</exception>
    public async Task CreateAsync(Account account, CancellationToken cancellationToken)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        await CheckNameUniqueness(account, cancellationToken);

        await _context.Accounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Асинхронно удаляет аккаунт по идентификатору и идентификатору пользователя.
    /// Помечает аккаунт как удаленный, не удаляя физически из БД.
    /// </summary>
    /// <param name="id">Идентификатор аккаунта.</param>
    /// <param name="userId">Идентификатор владельца аккаунта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если аккаунт не найден.</exception>
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (account == null)
            throw new EntityNotFoundException("Счет не найден");

        account.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Получает список активных аккаунтов пользователя с поддержкой пагинации.
    /// </summary>
    /// <param name="userId">Идентификатор владельца аккаунтов.</param>
    /// <param name="pageNumber">Номер страницы (по умолчанию 1).</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 100).</param>
    /// <returns>IQueryable<Account> - набор неотслеживаемых объектов аккаунтов.</returns>
    public IQueryable<Account> GetAllAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 100)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        return _context.Accounts
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderBy(a => a.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }

    /// <summary>
    /// Асинхронно получает аккаунт по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор аккаунта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Account? - объект аккаунта или null, если не найден.</returns>
    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Асинхронно обновляет существующий аккаунт.
    /// Проверяет уникальность имени, если оно изменилось.
    /// </summary>
    /// <param name="account">Объект аккаунта с обновленными данными.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="account"/> равен null.</exception>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если аккаунт не найден.</exception>
    /// <exception cref="EntityAlreadyExistsException">Выбрасывается, если новое имя уже занято.</exception>
    public async Task UpdateAsync(Account account, CancellationToken cancellationToken)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        var existingAccount = await _context.Accounts
            .FirstOrDefaultAsync(x => x.Id == account.Id, cancellationToken);

        if (existingAccount == null)
            throw new EntityNotFoundException("Счет не найден");

        if (!string.Equals(existingAccount.Name, account.Name, StringComparison.OrdinalIgnoreCase))
            await CheckNameUniqueness(account, cancellationToken);

        _context.Entry(existingAccount).CurrentValues.SetValues(account);
        _context.Entry(existingAccount).State = EntityState.Modified;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Проверяет уникальность имени аккаунта для текущего пользователя.
    /// </summary>
    /// <param name="account">Аккаунт для проверки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="EntityAlreadyExistsException">Выбрасывается, если имя уже занято.</exception>
    private async Task CheckNameUniqueness(Account account, CancellationToken cancellationToken)
    {
        var exists = false;

        var userAccounts = await _context.Accounts.Where(x => x.UserId == account.UserId && x.Id != account.Id).ToListAsync();
        exists = userAccounts.Any(x => x.Name.Equals(account.Name, StringComparison.OrdinalIgnoreCase));

        if (exists)
            throw new EntityAlreadyExistsException("Счет с таким именем уже существует у этого пользователя");
    }
}
