using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Domain.Entities;

namespace JoyGame.CaseStudy.Application.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<OperationResult<User?>> GetByUsernameAsync(string username);
    Task<OperationResult<User?>> GetByEmailAsync(string email);
    Task<OperationResult<bool>> IsUsernameUniqueAsync(string username);
    Task<OperationResult<bool>> IsEmailUniqueAsync(string email);
    Task<OperationResult<List<string>>> GetUserPermissionsAsync(int userId);
    Task<OperationResult<bool>> SaveResetTokenAsync(int userId, string token, DateTime expiresAt);
    Task<OperationResult<bool>> ValidateResetTokenAsync(string email, string token);
    Task<OperationResult<bool>> MarkResetTokenAsUsedAsync(string token);
}