namespace Prode.Application.Tournaments;

// Contratos de entrada y salida de los casos de uso de torneos.
// Los campos de entrada son anulables para poder validar archivos incompletos y responder con mensajes claros.
public sealed record ImportTournamentRequest(
    string? Name,
    IReadOnlyList<ImportTeamRequest?>? Teams,
    IReadOnlyList<ImportMatchRequest?>? Matches);

public sealed record ImportTeamRequest(string? Name, string? Code);

// La fecha llega como texto para exigir que venga en UTC (sufijo "Z" u offset +00:00).
public sealed record ImportMatchRequest(string? HomeCode, string? AwayCode, string? KickoffUtc, string? Stage);

public sealed record ImportTournamentResponse(int Id, string Name, int TeamsCount, int MatchesCount);

public sealed record TournamentDto(int Id, string Name, string Status);

public sealed record TeamDto(int Id, string Name, string Code);

public sealed record MatchDto(
    int Id,
    TeamDto HomeTeam,
    TeamDto AwayTeam,
    DateTime KickoffUtc,
    string Stage,
    string Status,
    int? HomeGoals,
    int? AwayGoals);
