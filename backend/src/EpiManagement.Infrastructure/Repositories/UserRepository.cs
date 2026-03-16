using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EpiManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _ctx;

    public UserRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Users.ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _ctx.Users.AddAsync(user, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _ctx.Users.Update(user);
        return Task.CompletedTask;
    }
}
