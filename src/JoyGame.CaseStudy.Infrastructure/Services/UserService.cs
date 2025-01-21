using System.Security.Cryptography;
using System.Text;
using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces.Repositories;
using JoyGame.CaseStudy.Application.Interfaces.Services;
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

    public async Task<OperationResult<UserDto?>> GetByIdAsync(int id)
    {
        var userOperationResult = await _userRepository.GetByIdAsync(id);

        if (!userOperationResult.IsSuccess)
            return OperationResult<UserDto?>.Failure(userOperationResult.ErrorCode, userOperationResult.ErrorMessage);

        return OperationResult<UserDto?>.Success(UserDto.MapToUserDto(userOperationResult.Data));
    }

    public async Task<OperationResult<UserDto?>> GetByUsernameAsync(string username)
    {
        var userOperationResult = await _userRepository.GetByUsernameAsync(username);

        if (!userOperationResult.IsSuccess)
            return OperationResult<UserDto?>.Failure(userOperationResult.ErrorCode, userOperationResult.ErrorMessage);

        return OperationResult<UserDto?>.Success(UserDto.MapToUserDto(userOperationResult.Data));
    }

    public async Task<OperationResult<List<UserDto>>> GetAllAsync()
    {
        var usersOperationResult = await _userRepository.GetAllAsync();

        if (!usersOperationResult.IsSuccess)
            return OperationResult<List<UserDto>>.Failure(usersOperationResult.ErrorCode,
                usersOperationResult.ErrorMessage);

        return OperationResult<List<UserDto>>.Success(usersOperationResult.Data.Select(UserDto.MapToUserDto).ToList());
    }

    public async Task<OperationResult<UserDto>> CreateAsync(CreateUserDto createUserDto)
    {
        if ((await _userRepository.IsUsernameUniqueAsync(createUserDto.Username)).Data == false)
        {
            _logger.LogWarning("Attempted to create user with existing username: {Username}", createUserDto.Username);
            throw new BusinessRuleException("Username is already taken");
        }

        if ((await _userRepository.IsEmailUniqueAsync(createUserDto.Email)).Data == false)
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

        var createdUserOperationResult = await _userRepository.AddAsync(user);

        if (!createdUserOperationResult.IsSuccess)
            return OperationResult<UserDto>.Failure(createdUserOperationResult.ErrorCode,
                createdUserOperationResult.ErrorMessage);

        _logger.LogInformation("Created new user with ID: {UserId}", createdUserOperationResult.Data.Id);

        return OperationResult<UserDto>.Success(UserDto.MapToUserDto(createdUserOperationResult.Data));
    }

    public async Task<OperationResult<UserDto>> UpdateAsync(int id, UpdateUserDto updateUserDto)
    {
        var userOperationResult = await _userRepository.GetByIdAsync(id);
        if (userOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to update non-existent user with ID: {UserId}", id);
            return OperationResult<UserDto>.Failure(userOperationResult.ErrorCode, userOperationResult.ErrorMessage);
        }

        if (!string.IsNullOrEmpty(updateUserDto.Email) &&
            userOperationResult.Data.Email != updateUserDto.Email &&
            (await _userRepository.IsEmailUniqueAsync(updateUserDto.Email)).Data == false)
        {
            _logger.LogWarning("Attempted to update user with existing email: {Email}", updateUserDto.Email);
            return OperationResult<UserDto>.Failure(ErrorCode.EmailExists, "Email is already registered");
        }

        if (!string.IsNullOrEmpty(updateUserDto.Email))
        {
            userOperationResult.Data.Email = updateUserDto.Email.ToLower();
        }

        if (!string.IsNullOrEmpty(updateUserDto.FirstName))
        {
            userOperationResult.Data.FirstName = updateUserDto.FirstName;
        }

        if (!string.IsNullOrEmpty(updateUserDto.LastName))
        {
            userOperationResult.Data.LastName = updateUserDto.LastName;
        }

        var updatedUserOperationResult = await _userRepository.UpdateAsync(userOperationResult.Data);

        if (!updatedUserOperationResult.IsSuccess)
            return OperationResult<UserDto>.Failure(updatedUserOperationResult.ErrorCode,
                updatedUserOperationResult.ErrorMessage);

        _logger.LogInformation("Updated user with ID: {UserId}", id);

        return OperationResult<UserDto>.Success(UserDto.MapToUserDto(updatedUserOperationResult.Data));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id)
    {
        var userOperationResult = await _userRepository.GetByIdAsync(id);
        if (userOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to delete non-existent user with ID: {UserId}", id);
            return OperationResult<bool>.Failure(userOperationResult.ErrorCode, userOperationResult.ErrorMessage);
        }

        var deleteOperationResult = await _userRepository.DeleteAsync(id);
        if (deleteOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Failed to delete user with ID: {UserId}", id);
            return OperationResult<bool>.Failure(deleteOperationResult.ErrorCode, deleteOperationResult.ErrorMessage);
        }

        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        var userOperationResult = await _userRepository.GetByIdAsync(userId);
        if (userOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to change password for non-existent user with ID: {UserId}", userId);
            return OperationResult<bool>.Failure(userOperationResult.ErrorCode, userOperationResult.ErrorMessage);
        }

        if (!VerifyPassword(changePasswordDto.CurrentPassword, userOperationResult.Data.PasswordHash))
        {
            _logger.LogWarning("Invalid current password provided for user ID: {UserId}", userId);
            return OperationResult<bool>.Failure(ErrorCode.InvalidPassword, "Invalid current password");
        }

        userOperationResult.Data.PasswordHash = HashPassword(changePasswordDto.NewPassword);
        var updateResult = await _userRepository.UpdateAsync(userOperationResult.Data);

        if (updateResult.IsSuccess == false)
            return OperationResult<bool>.Failure(updateResult.ErrorCode, updateResult.ErrorMessage);

        _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);

        return OperationResult<bool>.Success(true);
    }

    private static string HashPassword(string password)
    {
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