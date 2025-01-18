using JoyGame.CaseStudy.Domain.Entities;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<List<string>> GetUserPermissionsAsync(int userId);
}