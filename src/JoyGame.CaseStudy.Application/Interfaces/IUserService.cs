using JoyGame.CaseStudy.Application.DTOs;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> CreateAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
}