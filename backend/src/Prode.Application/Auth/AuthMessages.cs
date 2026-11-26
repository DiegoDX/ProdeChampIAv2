namespace Prode.Application.Auth;

// Mensajes que ve el usuario final.
public static class AuthMessages
{
    public const string ValidationFailed = "Hay datos inválidos en la solicitud.";
    public const string InvalidEmail = "El email no tiene un formato válido.";
    public const string InvalidDisplayName = "El nombre debe tener entre 2 y 40 caracteres.";
    public const string PasswordTooShort = "La contraseña debe tener al menos 8 caracteres.";
    public const string PasswordRequired = "La contraseña es obligatoria.";
    public const string EmailAlreadyRegistered = "Ya existe un usuario registrado con ese email.";

    // Mismo mensaje si el email no existe o si la contraseña es incorrecta, para no revelar qué emails están registrados.
    public const string InvalidCredentials = "Email o contraseña incorrectos.";
}
