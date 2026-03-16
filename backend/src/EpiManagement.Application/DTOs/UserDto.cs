using EpiManagement.Domain.Enums;

namespace EpiManagement.Application.DTOs;

public record UserDto(Guid Id, string Name, string Email, UserRole Role, bool IsActive);
public record CreateUserDto(string Name, string Email, string Password, UserRole Role);
public record UpdateUserDto(string Name, string Email, UserRole Role);
public record LoginDto(string Email, string Password);
public record LoginResultDto(string Token, string Name, string Email, UserRole Role, Guid UserId);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);
