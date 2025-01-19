using JoyGame.CaseStudy.Domain.Entities;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task<bool> SaveResetTokenAsync(int userId, string token, DateTime expiresAt);
    Task<bool> ValidateResetTokenAsync(string email, string token);
    Task<bool> MarkResetTokenAsUsedAsync(string token);
}