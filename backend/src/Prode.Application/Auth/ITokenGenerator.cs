using Prode.Domain.Entities;

namespace Prode.Application.Auth;

// Emite el token de acceso de un usuario autenticado.
public interface ITokenGenerator
{
    string Generate(User user);
}
