using EpiManagement.Application.DTOs;
using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EpiManagement.Application.Services;

public class AuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork uow, IConfiguration config)
    {
        _uow = uow;
        _config = config;
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email, ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Usuário inativo.");

        if (!VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var token = GenerateJwtToken(user);
        return new LoginResultDto(token, user.Name, user.Email, user.Role, user.Id);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Users.GetByEmailAsync(dto.Email, ct);
        if (existing is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var hash = HashPassword(dto.Password);
        var user = new User(dto.Name, dto.Email, hash, dto.Role);
        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return new UserDto(user.Id, user.Name, user.Email, user.Role, user.IsActive);
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await _uow.Users.GetAllAsync(ct);
        return users.Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role, u.IsActive));
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + "EpiSalt2024!");
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    public static bool VerifyPassword(string password, string hash)
        => HashPassword(password) == hash;
}
