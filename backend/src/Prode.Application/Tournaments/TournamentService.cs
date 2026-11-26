using System.Globalization;
using System.Text.RegularExpressions;
using Prode.Domain.Entities;
using Prode.Domain.Enums;
using Match = Prode.Domain.Entities.Match;

namespace Prode.Application.Tournaments;

// Casos de uso de torneos: importación desde archivo (solo administrador) y consultas de lectura.
public sealed partial class TournamentService(ITournamentRepository tournaments)
{
    public const int NameMaxLength = 100;

    // Valida todo el archivo antes de guardar; si hay algún error no se guarda nada.
    public async Task<TournamentResult<ImportTournamentResponse>> ImportAsync(
        ImportTournamentRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, List<string>>();
        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
            AddError(errors, "name", TournamentMessages.NameRequired);
        else if (name.Length > NameMaxLength)
            AddError(errors, "name", TournamentMessages.NameTooLong);

        var teams = ValidateTeams(request.Teams ?? [], errors);
        var matches = ValidateMatches(request.Matches ?? [], teams.Select(t => t.Code).ToHashSet(), errors);

        if (errors.Count > 0)
            return TournamentResult<ImportTournamentResponse>.Invalid(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));

        // Un equipo que ya existe (mismo código) se reutiliza: una selección juega varios torneos.
        var codes = teams.Select(t => t.Code).ToList();
        var existingTeams = await tournaments.GetTeamsByCodesAsync(codes, cancellationToken);
        var teamsByCode = existingTeams.ToDictionary(t => t.Code);
        foreach (var team in teams.Where(t => !teamsByCode.ContainsKey(t.Code)))
            teamsByCode[team.Code] = new Team { Name = team.Name, Code = team.Code };

        var tournament = new Tournament
        {
            Name = name,
            Status = TournamentStatus.Upcoming,
            Teams = codes.Select(code => new TournamentTeam { Team = teamsByCode[code] }).ToList(),
            Matches = matches.Select(m => new Match
            {
                HomeTeam = teamsByCode[m.HomeCode],
                AwayTeam = teamsByCode[m.AwayCode],
                KickoffUtc = m.KickoffUtc,
                Stage = m.Stage,
                Status = MatchStatus.Scheduled
            }).ToList()
        };

        await tournaments.AddAsync(tournament, cancellationToken);

        return TournamentResult<ImportTournamentResponse>.Success(
            new ImportTournamentResponse(tournament.Id, tournament.Name, tournament.Teams.Count, tournament.Matches.Count));
    }

    public async Task<IReadOnlyList<TournamentDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var list = await tournaments.ListAsync(cancellationToken);
        return list.Select(t => new TournamentDto(t.Id, t.Name, t.Status.ToString())).ToList();
    }

    // status es opcional: "Scheduled" o "Finished" (sin distinguir mayúsculas).
    public async Task<TournamentResult<IReadOnlyList<MatchDto>>> GetMatchesAsync(
        int tournamentId, string? status, CancellationToken cancellationToken = default)
    {
        MatchStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!TryParseEnumName<MatchStatus>(status, out var parsed))
            {
                return TournamentResult<IReadOnlyList<MatchDto>>.Invalid(
                    new Dictionary<string, string[]> { ["status"] = [TournamentMessages.MatchStatusInvalid] },
                    TournamentMessages.MatchStatusInvalid);
            }
            statusFilter = parsed;
        }

        if (!await tournaments.ExistsAsync(tournamentId, cancellationToken))
            return TournamentResult<IReadOnlyList<MatchDto>>.NotFound(TournamentMessages.TournamentNotFound);

        var matches = await tournaments.GetMatchesAsync(tournamentId, statusFilter, cancellationToken);
        return TournamentResult<IReadOnlyList<MatchDto>>.Success(matches.Select(ToDto).ToList());
    }

    private static List<ValidTeam> ValidateTeams(IReadOnlyList<ImportTeamRequest?> teams, Dictionary<string, List<string>> errors)
    {
        if (teams.Count < 2)
            AddError(errors, "teams", TournamentMessages.TeamsRequired);

        var valid = new List<ValidTeam>();
        var seenCodes = new HashSet<string>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < teams.Count; i++)
        {
            var name = teams[i]?.Name?.Trim() ?? string.Empty;
            var code = NormalizeCode(teams[i]?.Code);
            var isValid = true;

            if (name.Length == 0)
            {
                AddError(errors, $"teams[{i}].name", TournamentMessages.TeamNameRequired);
                isValid = false;
            }
            else if (name.Length > NameMaxLength)
            {
                AddError(errors, $"teams[{i}].name", TournamentMessages.TeamNameTooLong);
                isValid = false;
            }
            else if (!seenNames.Add(name))
            {
                AddError(errors, $"teams[{i}].name", TournamentMessages.DuplicateTeamName(name));
                isValid = false;
            }

            if (!TeamCodeRegex().IsMatch(code))
            {
                AddError(errors, $"teams[{i}].code", TournamentMessages.TeamCodeInvalid);
                isValid = false;
            }
            else if (!seenCodes.Add(code))
            {
                AddError(errors, $"teams[{i}].code", TournamentMessages.DuplicateTeamCode(code));
                isValid = false;
            }

            if (isValid)
                valid.Add(new ValidTeam(name, code));
        }

        return valid;
    }

    private static List<ValidMatch> ValidateMatches(
        IReadOnlyList<ImportMatchRequest?> matches, HashSet<string> teamCodes, Dictionary<string, List<string>> errors)
    {
        if (matches.Count == 0)
            AddError(errors, "matches", TournamentMessages.MatchesRequired);

        var valid = new List<ValidMatch>();

        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var homeCode = NormalizeCode(match?.HomeCode);
            var awayCode = NormalizeCode(match?.AwayCode);
            var isValid = ValidateMatchTeam(homeCode, $"matches[{i}].homeCode", teamCodes, errors);
            isValid &= ValidateMatchTeam(awayCode, $"matches[{i}].awayCode", teamCodes, errors);

            if (isValid && homeCode == awayCode)
            {
                AddError(errors, $"matches[{i}].awayCode", TournamentMessages.TeamPlaysItself);
                isValid = false;
            }

            if (!TryParseUtc(match?.KickoffUtc, out var kickoffUtc))
            {
                AddError(errors, $"matches[{i}].kickoffUtc", TournamentMessages.KickoffNotUtc);
                isValid = false;
            }

            if (!TryParseEnumName<MatchStage>(match?.Stage, out var stage))
            {
                AddError(errors, $"matches[{i}].stage", TournamentMessages.StageInvalid);
                isValid = false;
            }

            if (isValid)
                valid.Add(new ValidMatch(homeCode, awayCode, kickoffUtc, stage));
        }

        return valid;
    }

    private static bool ValidateMatchTeam(string code, string key, HashSet<string> teamCodes, Dictionary<string, List<string>> errors)
    {
        if (code.Length == 0)
        {
            AddError(errors, key, TournamentMessages.MatchTeamCodeRequired);
            return false;
        }
        if (!teamCodes.Contains(code))
        {
            AddError(errors, key, TournamentMessages.UnknownTeamCode(code));
            return false;
        }
        return true;
    }

    // Exige una zona horaria explícita y que sea UTC ("Z" u offset cero). Sin zona, la fecha sería ambigua.
    private static bool TryParseUtc(string? value, out DateTime utc)
    {
        utc = default;
        if (string.IsNullOrWhiteSpace(value) || !UtcDesignatorRegex().IsMatch(value))
            return false;
        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            || parsed.Offset != TimeSpan.Zero)
            return false;

        utc = parsed.UtcDateTime;
        return true;
    }

    // Acepta solo el nombre del valor (sin distinguir mayúsculas); rechaza números como "1".
    private static bool TryParseEnumName<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        result = default;
        var name = Enum.GetNames<TEnum>().FirstOrDefault(n => string.Equals(n, value?.Trim(), StringComparison.OrdinalIgnoreCase));
        return name is not null && Enum.TryParse(name, out result);
    }

    private static string NormalizeCode(string? code) => code?.Trim().ToUpperInvariant() ?? string.Empty;

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var list))
            errors[key] = list = [];
        list.Add(message);
    }

    private static MatchDto ToDto(Match match) => new(
        match.Id,
        ToDto(match.HomeTeam),
        ToDto(match.AwayTeam),
        match.KickoffUtc,
        match.Stage.ToString(),
        match.Status.ToString(),
        match.HomeGoals,
        match.AwayGoals);

    private static TeamDto ToDto(Team team) => new(team.Id, team.Name, team.Code);

    private sealed record ValidTeam(string Name, string Code);

    private sealed record ValidMatch(string HomeCode, string AwayCode, DateTime KickoffUtc, MatchStage Stage);

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex TeamCodeRegex();

    // Termina en "Z" o en un offset del tipo +00:00.
    [GeneratedRegex(@"(?:Z|[+-]\d{2}:\d{2})$", RegexOptions.IgnoreCase)]
    private static partial Regex UtcDesignatorRegex();
}
