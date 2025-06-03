using HomeBudget.AuthService.EF.Data;
using HomeBudget.AuthService.EF.Models;
using HomeBudget.AuthService.EF.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeBudget.AuthService.EF.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Users
            .Where(u => u.Id == id && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetByLoginAsync(string login, CancellationToken ct)
    {
        return await _context.Users
            .Where(u => u.Login == login && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        var userToUpdate = await _context.Users
            .Where(u => u.Id == user.Id && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (userToUpdate == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        userToUpdate.Login = user.Login;
        userToUpdate.Email = user.Email;
        userToUpdate.FirstName = user.FirstName;
        userToUpdate.LastName = user.LastName;
        userToUpdate.BirthDate = user.BirthDate;

        await _context.SaveChangesAsync(ct);
    }
}