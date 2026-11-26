namespace Prode.Application.Tournaments;

public enum TournamentError
{
    None,
    Validation,
    NotFound
}

// Resultado de un caso de uso de torneos. La Api lo traduce al código HTTP correspondiente.
public sealed class TournamentResult<T>
{
    private TournamentResult(T? value, TournamentError error, string? message, IReadOnlyDictionary<string, string[]>? validationErrors)
    {
        Value = value;
        Error = error;
        Message = message;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public T? Value { get; }
    public TournamentError Error { get; }
    public string? Message { get; }
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }
    public bool Succeeded => Error == TournamentError.None;

    public static TournamentResult<T> Success(T value) => new(value, TournamentError.None, null, null);

    public static TournamentResult<T> Invalid(IReadOnlyDictionary<string, string[]> errors, string message = TournamentMessages.ValidationFailed) =>
        new(default, TournamentError.Validation, message, errors);

    public static TournamentResult<T> NotFound(string message) => new(default, TournamentError.NotFound, message, null);
}
