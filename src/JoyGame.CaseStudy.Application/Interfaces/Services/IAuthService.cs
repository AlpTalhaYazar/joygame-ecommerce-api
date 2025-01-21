using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;

namespace JoyGame.CaseStudy.Application.Interfaces.Services;

public interface IAuthService
{
    Task<OperationResult<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<OperationResult<bool>> ValidateTokenAsync(string token);
    Task<OperationResult<ForgotPasswordResponseDto>> ForgotPasswordAsync(string email);
    Task<OperationResult<bool>> ResetPasswordAsync(ResetPasswordDto request);
}