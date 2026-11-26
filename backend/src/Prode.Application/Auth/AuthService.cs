using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Prode.Domain.Entities;

namespace Prode.Application.Auth;

// Casos de uso de registro, login y consulta del usuario autenticado.
public sealed partial class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IOptions<AdminOptions> adminOptions,
    TimeProvider timeProvider)
{
    public const int DisplayNameMinLength = 2;
    public const int DisplayNameMaxLength = 40;
    public const int PasswordMinLength = 8;
    private const int EmailMaxLength = 256;

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        var errors = new Dictionary<string, string[]>();
        if (!IsValidEmail(email))
            errors["email"] = [AuthMessages.InvalidEmail];
        if (displayName.Length is < DisplayNameMinLength or > DisplayNameMaxLength)
            errors["displayName"] = [AuthMessages.InvalidDisplayName];
        if (password.Length < PasswordMinLength)
            errors["password"] = [AuthMessages.PasswordTooShort];
        if (errors.Count > 0)
            return AuthResult.Invalid(errors);

        if (await users.ExistsByEmailAsync(email, cancellationToken))
            return AuthResult.Failure(AuthError.EmailAlreadyRegistered, AuthMessages.EmailAlreadyRegistered);

        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            IsAdmin = IsAdminEmail(email),
            IsActive = true,
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime
        };
        user.PasswordHash = passwordHasher.Hash(user, password);

        await users.AddAsync(user, cancellationToken);

        return AuthResult.Success(CreateResponse(user));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var password = request.Password ?? string.Empty;

        var errors = new Dictionary<string, string[]>();
        if (!IsValidEmail(email))
            errors["email"] = [AuthMessages.InvalidEmail];
        if (password.Length == 0)
            errors["password"] = [AuthMessages.PasswordRequired];
        if (errors.Count > 0)
            return AuthResult.Invalid(errors);

        var user = await users.GetByEmailAsync(email, cancellationToken);

        // Usuario inexistente, dado de baja, sin contraseña (solo Google) o contraseña incorrecta: misma respuesta.
        if (user is null
            || !user.IsActive
            || user.PasswordHash is null
            || !passwordHasher.Verify(user, user.PasswordHash, password))
        {
            return AuthResult.Failure(AuthError.InvalidCredentials, AuthMessages.InvalidCredentials);
        }

        return AuthResult.Success(CreateResponse(user));
    }

    // Devuelve null si el usuario del token ya no existe o fue dado de baja.
    public async Task<UserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        return user is { IsActive: true } ? ToDto(user) : null;
    }

    public static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

    private bool IsAdminEmail(string email)
    {
        var adminEmail = NormalizeEmail(adminOptions.Value.Email);
        return adminEmail.Length > 0 && adminEmail == email;
    }

    private static bool IsValidEmail(string email) =>
        email.Length <= EmailMaxLength && EmailRegex().IsMatch(email);

    private AuthResponse CreateResponse(User user) => new(tokenGenerator.Generate(user), ToDto(user));

    private static UserDto ToDto(User user) => new(user.Id, user.Email, user.DisplayName, user.IsAdmin);

    // Formato básico: texto@dominio.ext, sin espacios y con una sola arroba.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
