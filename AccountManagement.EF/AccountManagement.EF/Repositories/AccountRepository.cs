using AccountManagement.EF.Exceptions;
using AccountManagement.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.EF.Repositories.Interfaces;

public class AccountRepository : IAccountRepository
{
    private readonly AccountManagementContext _context;

    public AccountRepository(AccountManagementContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task CreateAsync(Account account, Guid userId, CancellationToken cancellationToken)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (account.UserId != userId)
            throw new ArgumentException("Неверный пользователь", nameof(userId));

        await CheckNameUniqueness(account, cancellationToken);

        await _context.Accounts.AddAsync(account, cancellationToken);

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

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (account == null)
            throw new EntityNotFoundException("Счет не найден");

        account.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

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

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

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

    private async Task CheckNameUniqueness(Account account, CancellationToken cancellationToken)
    {
        var exists = await _context.Accounts
            .AnyAsync(x => x.Name.Equals(account.Name, StringComparison.OrdinalIgnoreCase)
                         && x.UserId == account.UserId
                         && x.Id != account.Id,
                         cancellationToken);

        if (exists)
            throw new EntityAlreadyExistsException("Счет с таким именем уже существует у этого пользователя");
    }
}
