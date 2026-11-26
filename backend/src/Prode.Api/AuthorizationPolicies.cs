namespace Prode.Api;

// Nombres de las políticas de autorización.
public static class AuthorizationPolicies
{
    // Solo el administrador global (claim de rol "Admin"). Se usa en la carga de torneos y resultados.
    public const string AdminOnly = "AdminOnly";
}
