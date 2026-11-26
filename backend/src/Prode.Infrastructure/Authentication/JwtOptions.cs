namespace Prode.Infrastructure.Authentication;

// Configuración del token JWT (sección "Jwt").
// La clave de firma se configura con user-secrets o variables de entorno; nunca en el repositorio.
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    // HMAC-SHA256 necesita una clave de al menos 256 bits.
    public const int MinSigningKeyLength = 32;

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Issuer)
        && !string.IsNullOrWhiteSpace(Audience)
        && SigningKey.Length >= MinSigningKeyLength;
}
