using Microsoft.EntityFrameworkCore;
using Prode.Application.Auth;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ProdeDbContext context) : IUserRepository
{
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        context.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}
