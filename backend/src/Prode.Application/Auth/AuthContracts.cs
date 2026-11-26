namespace Prode.Application.Auth;

// Contratos de entrada y salida de los casos de uso de autenticación.
// Los campos de entrada son anulables para poder validar cuerpos incompletos y responder con mensajes claros.
public sealed record RegisterRequest(string? Email, string? DisplayName, string? Password);

public sealed record LoginRequest(string? Email, string? Password);

public sealed record UserDto(int Id, string Email, string DisplayName, bool IsAdmin);

public sealed record AuthResponse(string Token, UserDto User);
