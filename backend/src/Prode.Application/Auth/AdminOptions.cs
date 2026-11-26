namespace Prode.Application.Auth;

// Configuración del administrador global (sección "Admin").
// El usuario que se registra con este email queda como administrador.
public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public string Email { get; set; } = string.Empty;
}
