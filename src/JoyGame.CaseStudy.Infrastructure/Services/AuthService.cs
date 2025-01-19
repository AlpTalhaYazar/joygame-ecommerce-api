using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;
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

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            throw new BusinessRuleException("Invalid username or password");
        }

        if (user.Status != EntityStatus.Active)
        {
            _logger.LogWarning("Login attempt for inactive user: {Username}", request.Username);
            throw new BusinessRuleException("Account is not active");
        }

        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id);

        var token = GenerateJwtToken(user, permissions);

        return new AuthResponseDto
        {
            Token = token,
            User = MapToUserDto(user)
        };
    }

    public async Task<bool> ValidateTokenAsync(string token)
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
            return false;
        }

        return true;
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        // Always return true even if email doesn't exist (security best practice)
        if (user == null)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
            return new()
            {
                ResetToken = string.Empty,
                Email = email
            };
        }

        // Generate password reset token
        var resetToken = GeneratePasswordResetToken();

        // In a real application, save this token to the database
        // await _userRepository.SaveResetTokenAsync(user.Id, resetToken);

        return new()
        {
            ResetToken = resetToken,
            Email = email
        };
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto request)
    {
        throw new NotImplementedException();
    }

    private string GenerateJwtToken(User user, List<string> permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
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

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string GeneratePasswordResetToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BusinessStatus = user.BusinessStatus,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };
    }
}