using Prode.Domain.Entities;

namespace Prode.Application.Auth;

// Acceso a los usuarios persistidos. El email se recibe ya normalizado.
public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
}
