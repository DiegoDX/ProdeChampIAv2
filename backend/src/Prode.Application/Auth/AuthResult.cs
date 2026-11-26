namespace Prode.Application.Auth;

public enum AuthError
{
    None,
    Validation,
    EmailAlreadyRegistered,
    InvalidCredentials
}

// Resultado de un caso de uso de autenticación. La Api lo traduce al código HTTP correspondiente.
public sealed class AuthResult
{
    private AuthResult(AuthResponse? response, AuthError error, string? message, IReadOnlyDictionary<string, string[]>? validationErrors)
    {
        Response = response;
        Error = error;
        Message = message;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public AuthResponse? Response { get; }
    public AuthError Error { get; }
    public string? Message { get; }
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }
    public bool Succeeded => Error == AuthError.None;

    public static AuthResult Success(AuthResponse response) => new(response, AuthError.None, null, null);

    public static AuthResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(null, AuthError.Validation, AuthMessages.ValidationFailed, errors);

    public static AuthResult Failure(AuthError error, string message) => new(null, error, message, null);
}
