using JoyGame.CaseStudy.Application.DTOs;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<bool> ValidateTokenAsync(string token);
    Task<ForgotPasswordResponseDto> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto request);
}