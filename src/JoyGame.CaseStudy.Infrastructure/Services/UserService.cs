using System.Security.Cryptography;
using System.Text;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JoyGame.CaseStudy.Infrastructure.Services;

public class UserService(
    IUserRepository userRepository,
    ILogger<UserService> logger)
    : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<UserService> _logger = logger;

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? UserDto.MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? UserDto.MapToUserDto(user) : null;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(UserDto.MapToUserDto).ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
    {
        if (await _userRepository.IsUsernameUniqueAsync(createUserDto.Username) == false)
        {
            _logger.LogWarning("Attempted to create user with existing username: {Username}", createUserDto.Username);
            throw new BusinessRuleException("Username is already taken");
        }

        if (await _userRepository.IsEmailUniqueAsync(createUserDto.Email) == false)
        {
            _logger.LogWarning("Attempted to create user with existing email: {Email}", createUserDto.Email);
            throw new BusinessRuleException("Email is already registered");
        }

        var user = new User
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email.ToLower(),
            PasswordHash = HashPassword(createUserDto.Password),
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Status = EntityStatus.Active,
            BusinessStatus = UserStatus.Active,
            CreatedBy = "System",
        };

        var createdUser = await _userRepository.AddAsync(user);
        _logger.LogInformation("Created new user with ID: {UserId}", createdUser.Id);

        return UserDto.MapToUserDto(createdUser);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Attempted to update non-existent user with ID: {UserId}", id);
            throw new EntityNotFoundException(nameof(User), id);
        }

        if (!string.IsNullOrEmpty(updateUserDto.Email) &&
            user.Email != updateUserDto.Email &&
            !await _userRepository.IsEmailUniqueAsync(updateUserDto.Email))
        {
            throw new BusinessRuleException("Email is already registered");
        }

        if (!string.IsNullOrEmpty(updateUserDto.Email))
        {
            user.Email = updateUserDto.Email.ToLower();
        }
        if (!string.IsNullOrEmpty(updateUserDto.FirstName))
        {
            user.FirstName = updateUserDto.FirstName;
        }
        if (!string.IsNullOrEmpty(updateUserDto.LastName))
        {
            user.LastName = updateUserDto.LastName;
        }

        var updatedUser = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Updated user with ID: {UserId}", id);

        return UserDto.MapToUserDto(updatedUser);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Attempted to delete non-existent user with ID: {UserId}", id);
            return false;
        }

        var result = await _userRepository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation("Deleted user with ID: {UserId}", id);
        }

        return result;
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Attempted to change password for non-existent user with ID: {UserId}", userId);
            throw new EntityNotFoundException(nameof(User), userId);
        }

        if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Invalid current password provided for user ID: {UserId}", userId);
            throw new BusinessRuleException("Current password is incorrect");
        }

        user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
        return true;
    }

    private static string HashPassword(string password)
    {
        // In a real application, use a proper password hashing library like BCrypt
        return Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var inputHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        return inputHash == hash;
    }
}