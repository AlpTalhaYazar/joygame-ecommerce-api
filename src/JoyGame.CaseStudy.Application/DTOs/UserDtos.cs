using JoyGame.CaseStudy.Domain.Enums;

namespace JoyGame.CaseStudy.Application.DTOs;

public record UserDto
{
    public int Id { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public UserStatus BusinessStatus { get; init; }
    public List<string> Roles { get; init; } = new();
}

public record CreateUserDto
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

public record UpdateUserDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
}

public record ChangePasswordDto
{
    public string CurrentPassword { get; init; }
    public string NewPassword { get; init; }
}