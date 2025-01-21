using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;

namespace JoyGame.CaseStudy.Application.Interfaces.Services;

public interface IUserService
{
    Task<OperationResult<UserDto?>> GetByIdAsync(int id);
    Task<OperationResult<UserDto?>> GetByUsernameAsync(string username);
    Task<OperationResult<List<UserDto>>> GetAllAsync();
    Task<OperationResult<UserDto>> CreateAsync(CreateUserDto createUserDto);
    Task<OperationResult<UserDto>> UpdateAsync(int id, UpdateUserDto updateUserDto);
    Task<OperationResult<bool>> DeleteAsync(int id);
    Task<OperationResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
}