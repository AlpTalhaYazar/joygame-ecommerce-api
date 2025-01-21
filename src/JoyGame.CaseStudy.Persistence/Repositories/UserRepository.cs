using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.Interfaces.Repositories;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JoyGame.CaseStudy.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context) : BaseRepository<User>(context), IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<OperationResult<User?>> GetByUsernameAsync(string username)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return OperationResult<User?>.Failure(ErrorCode.UserNotFound, "User not found");

        return OperationResult<User?>.Success(user);
    }

    public async Task<OperationResult<User?>> GetByEmailAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
            return OperationResult<User?>.Failure(ErrorCode.UserNotFound, "User not found");

        return OperationResult<User?>.Success(user);
    }

    public async Task<OperationResult<bool>> IsUsernameUniqueAsync(string username)
    {
        var isUsernameUnique = !await _context.Users
            .AnyAsync(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        return OperationResult<bool>.Success(isUsernameUnique);
    }

    public async Task<OperationResult<bool>> IsEmailUniqueAsync(string email)
    {
        var isEmailUnique = !await _context.Users
            .AnyAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        return OperationResult<bool>.Success(isEmailUnique);
    }

    public async Task<OperationResult<List<string>>> GetUserPermissionsAsync(int userId)
    {
        var permissions = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        if (permissions.Count == 0)
            return OperationResult<List<string>>.Failure(ErrorCode.PermissionNotFound, "No permissions found");

        return OperationResult<List<string>>.Success(permissions);
    }

    public async Task<OperationResult<bool>> SaveResetTokenAsync(int userId, string token, DateTime expiresAt)
    {
        var resetToken = new PasswordResetToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsUsed = false,
            CreatedBy = userId.ToString()
        };

        var addResult = await _context.Set<PasswordResetToken>().AddAsync(resetToken);

        if (addResult.State != EntityState.Added)
            return OperationResult<bool>.Failure(ErrorCode.DatabaseError, "Error adding entity");

        var saveResult = await _context.SaveChangesAsync();

        if (saveResult == 0)
            return OperationResult<bool>.Failure(ErrorCode.DatabaseError, "Error saving entity");

        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<bool>> ValidateResetTokenAsync(string email, string token)
    {
        var userOperationResult = await GetByEmailAsync(email);

        if (!userOperationResult.IsSuccess)
            return OperationResult<bool>.Failure(userOperationResult.ErrorCode, userOperationResult.ErrorMessage);

        var isResetTokenValid = await _context.Set<PasswordResetToken>()
            .AnyAsync(rt =>
                rt.UserId == userOperationResult.Data.Id &&
                rt.Token == token &&
                rt.ExpiresAt > DateTime.UtcNow &&
                !rt.IsUsed);

        if (!isResetTokenValid)
            return OperationResult<bool>.Failure(ErrorCode.InvalidToken, "Token is invalid");

        return OperationResult<bool>.Success(isResetTokenValid);
    }

    public async Task<OperationResult<bool>> MarkResetTokenAsUsedAsync(string token)
    {
        var resetToken = await _context.Set<PasswordResetToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (resetToken == null)
            return OperationResult<bool>.Failure(ErrorCode.TokenNotFound, "Token not found");

        resetToken.IsUsed = true;
        var saveResult = await _context.SaveChangesAsync();

        if (saveResult == 0)
            return OperationResult<bool>.Failure(ErrorCode.DatabaseError, "Error saving entity");

        return OperationResult<bool>.Success(true);
    }
}