using Prode.Domain.Entities;

namespace Prode.Application.Auth;

// Genera y verifica hashes de contraseñas. Nunca se guarda la contraseña en texto plano.
public interface IPasswordHasher
{
    string Hash(User user, string password);
    bool Verify(User user, string passwordHash, string password);
}
