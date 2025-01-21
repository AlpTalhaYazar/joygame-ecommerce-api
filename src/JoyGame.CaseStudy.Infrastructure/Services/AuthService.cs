using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces.Repositories;
using JoyGame.CaseStudy.Application.Interfaces.Services;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace JoyGame.CaseStudy.Infrastructure.Services;

public class AuthService(
    IUserRepository userRepository,
    IConfiguration configuration,
    ILogger<AuthService> logger)
    : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<OperationResult<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var userOperationResult = await _userRepository.GetByUsernameAsync(request.Username);

        if (userOperationResult.IsSuccess == false
            || userOperationResult.Data == null
            || !VerifyPassword(request.Password, userOperationResult.Data.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            return OperationResult<AuthResponseDto>.Failure(ErrorCode.InvalidCredentials,
                "Invalid username or password");
        }

        if (userOperationResult.Data.Status != EntityStatus.Active)
        {
            _logger.LogWarning("Login attempt for inactive user: {Username}", request.Username);
            return OperationResult<AuthResponseDto>.Failure(ErrorCode.UserInactive, "User is inactive");
        }

        var permissionsOperationResult = await _userRepository.GetUserPermissionsAsync(userOperationResult.Data.Id);

        if (!permissionsOperationResult.IsSuccess)
        {
            _logger.LogError("Failed to retrieve permissions for user: {Username}", request.Username);
            return OperationResult<AuthResponseDto>.Failure(permissionsOperationResult.ErrorCode,
                permissionsOperationResult.ErrorMessage);
        }

        var token = GenerateJwtToken(userOperationResult.Data, permissionsOperationResult.Data);

        var response = new AuthResponseDto
        {
            Token = token,
            User = UserDto.MapToUserDto(userOperationResult.Data)
        };

        return OperationResult<AuthResponseDto>.Success(response);
    }

    public async Task<OperationResult<bool>> ValidateTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        try
        {
            tokenHandler.ValidateToken(token, new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);
        }
        catch
        {
            return OperationResult<bool>.Failure(ErrorCode.InvalidToken, "Invalid token");
        }

        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<ForgotPasswordResponseDto>> ForgotPasswordAsync(string email)
    {
        var userOperationResult = await _userRepository.GetByEmailAsync(email);

        if (userOperationResult.IsSuccess == false)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
            return OperationResult<ForgotPasswordResponseDto>.Success(
                new() { ResetToken = String.Empty, Email = email });
        }

        var resetToken = GeneratePasswordResetToken();

        var saveResetTokenOperationResult = await _userRepository.SaveResetTokenAsync(userOperationResult.Data.Id,
            resetToken, DateTime.UtcNow.AddHours(1));

        if (saveResetTokenOperationResult.IsSuccess == false)
        {
            _logger.LogError("Failed to save reset token for user: {Username}", userOperationResult.Data.Username);
            return OperationResult<ForgotPasswordResponseDto>.Failure(saveResetTokenOperationResult.ErrorCode,
                saveResetTokenOperationResult.ErrorMessage);
        }

        var response = new ForgotPasswordResponseDto()
        {
            ResetToken = resetToken,
            ExpiryDate = DateTime.UtcNow.AddHours(1),
            Email = email
        };

        _logger.LogInformation("Password reset requested for email: {Email}", email);
        return OperationResult<ForgotPasswordResponseDto>.Success(response);
    }

    public async Task<OperationResult<bool>> ResetPasswordAsync(ResetPasswordDto request)
    {
        if ((await _userRepository.ValidateResetTokenAsync(request.Email, request.ResetToken)).IsSuccess == false)
        {
            return OperationResult<bool>.Failure(ErrorCode.InvalidToken, "Invalid reset token");
        }

        var userOperationResult = await _userRepository.GetByEmailAsync(request.Email);

        if (userOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to reset password for non-existent email: {Email}", request.Email);
            return OperationResult<bool>.Failure(ErrorCode.UserNotFound, "User not found");
        }

        userOperationResult.Data.PasswordHash = HashPassword(request.NewPassword);
        var updateOperationResult = await _userRepository.UpdateAsync(userOperationResult.Data);

        if (updateOperationResult.IsSuccess == false)
        {
            _logger.LogError("Failed to reset password for user: {Username}", userOperationResult.Data.Username);
            return OperationResult<bool>.Failure(updateOperationResult.ErrorCode, updateOperationResult.ErrorMessage);
        }

        var markResetTokenAsUsedOperationResult = await _userRepository.MarkResetTokenAsUsedAsync(request.ResetToken);

        if (markResetTokenAsUsedOperationResult.IsSuccess == false)
        {
            _logger.LogError("Failed to mark reset token as used for user: {Username}",
                userOperationResult.Data.Username);
        }

        _logger.LogInformation("Password reset successful for user: {Username}", userOperationResult.Data.Username);
        return OperationResult<bool>.Success(true);
    }

    private string GenerateJwtToken(User user, List<string> permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var claims = new List<Claim>
        {
            new("id", user.Id.ToString()),
            new("username", user.Username),
            new(ClaimTypes.Email, user.Email),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
        };

        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(100),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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

    private static string GeneratePasswordResetToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}