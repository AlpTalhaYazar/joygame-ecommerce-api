using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JoyGame.CaseStudy.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context) : BaseRepository<User>(context), IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> IsUsernameUniqueAsync(string username)
    {
        return !await _context.Users
            .AnyAsync(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Users
            .AnyAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var permissions = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    public async Task<bool> SaveResetTokenAsync(int userId, string token, DateTime expiresAt)
    {
        var resetToken = new PasswordResetToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsUsed = false,
            CreatedBy = userId.ToString()
        };

        await _context.Set<PasswordResetToken>().AddAsync(resetToken);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidateResetTokenAsync(string email, string token)
    {
        var user = await GetByEmailAsync(email);
        if (user == null) return false;

        var isResetTokenValid = await _context.Set<PasswordResetToken>()
            .AnyAsync(rt =>
                rt.UserId == user.Id &&
                rt.Token == token &&
                rt.ExpiresAt > DateTime.UtcNow &&
                !rt.IsUsed);

        return isResetTokenValid;
    }

    public async Task<bool> MarkResetTokenAsUsedAsync(string token)
    {
        var resetToken = await _context.Set<PasswordResetToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (resetToken == null) return false;

        resetToken.IsUsed = true;
        await _context.SaveChangesAsync();
        return true;
    }
}