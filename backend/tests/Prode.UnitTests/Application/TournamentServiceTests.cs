using Prode.Application.Tournaments;
using Prode.Domain.Entities;
using Prode.Domain.Enums;

namespace Prode.UnitTests.Application;

// Prueba la importación y la lectura de torneos con un repositorio en memoria.
public sealed class TournamentServiceTests
{
    private readonly FakeTournamentRepository _repository = new();
    private readonly TournamentService _service;

    public TournamentServiceTests()
    {
        _service = new TournamentService(_repository);
    }

    private static ImportTournamentRequest ValidRequest() => new(
        "Copa de Prueba",
        [
            new ImportTeamRequest("Equipo A", "EQA"),
            new ImportTeamRequest("Equipo B", "EQB"),
            new ImportTeamRequest("Equipo C", "EQC")
        ],
        [
            new ImportMatchRequest("EQA", "EQB", "2026-06-15T19:00:00Z", "Group"),
            new ImportMatchRequest("EQB", "EQC", "2026-06-16T19:00:00Z", "Group"),
            new ImportMatchRequest("EQC", "EQA", "2026-06-17T19:00:00+00:00", "Knockout")
        ]);

    [Fact]
    public async Task Import_ValidFile_CreatesTournamentTeamsAndMatches()
    {
        var result = await _service.ImportAsync(ValidRequest());

        Assert.True(result.Succeeded);
        var tournament = Assert.Single(_repository.Tournaments);
        Assert.Equal("Copa de Prueba", tournament.Name);
        Assert.Equal(TournamentStatus.Upcoming, tournament.Status);
        Assert.Equal(3, tournament.Teams.Count);
        Assert.Equal(3, tournament.Matches.Count);
        Assert.Equal(new ImportTournamentResponse(tournament.Id, "Copa de Prueba", 3, 3), result.Value);
    }

    [Fact]
    public async Task Import_ValidFile_MapsMatchTeamsDatesAndStages()
    {
        await _service.ImportAsync(ValidRequest());

        var matches = _repository.Tournaments.Single().Matches.ToList();
        Assert.Equal("EQA", matches[0].HomeTeam.Code);
        Assert.Equal("EQB", matches[0].AwayTeam.Code);
        Assert.Equal(new DateTime(2026, 6, 15, 19, 0, 0, DateTimeKind.Utc), matches[0].KickoffUtc);
        Assert.Equal(DateTimeKind.Utc, matches[0].KickoffUtc.Kind);
        Assert.Equal(new DateTime(2026, 6, 17, 19, 0, 0, DateTimeKind.Utc), matches[2].KickoffUtc);
        Assert.Equal(MatchStage.Group, matches[0].Stage);
        Assert.Equal(MatchStage.Knockout, matches[2].Stage);
        Assert.All(matches, m => Assert.Equal(MatchStatus.Scheduled, m.Status));
        Assert.All(matches, m => Assert.Null(m.HomeGoals));
    }

    [Fact]
    public async Task Import_NormalizesCodesToUpperCase()
    {
        var request = ValidRequest() with
        {
            Teams = [new ImportTeamRequest("Equipo A", "eqa"), new ImportTeamRequest("Equipo B", "EQB")],
            Matches = [new ImportMatchRequest("EQA", "eqb", "2026-06-15T19:00:00Z", "group")]
        };

        var result = await _service.ImportAsync(request);

        Assert.True(result.Succeeded);
        var tournament = _repository.Tournaments.Single();
        Assert.Contains(tournament.Teams, tt => tt.Team.Code == "EQA");
        Assert.Equal("EQB", tournament.Matches.Single().AwayTeam.Code);
    }

    [Fact]
    public async Task Import_TeamCodeAlreadyStored_ReusesExistingTeam()
    {
        var existing = new Team { Id = 99, Name = "Equipo A", Code = "EQA" };
        _repository.StoredTeams.Add(existing);

        await _service.ImportAsync(ValidRequest());

        var tournament = _repository.Tournaments.Single();
        Assert.Same(existing, tournament.Teams.Single(tt => tt.Team.Code == "EQA").Team);
        Assert.Same(existing, tournament.Matches.First().HomeTeam);
    }

    [Fact]
    public async Task Import_UnknownTeamCode_ReturnsValidationErrorAndSavesNothing()
    {
        var request = ValidRequest() with
        {
            Matches = [new ImportMatchRequest("EQA", "XYZ", "2026-06-15T19:00:00Z", "Group")]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.UnknownTeamCode("XYZ")], result.ValidationErrors["matches[0].awayCode"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Fact]
    public async Task Import_TeamPlaysItself_ReturnsValidationErrorAndSavesNothing()
    {
        var request = ValidRequest() with
        {
            Matches = [new ImportMatchRequest("EQA", "EQA", "2026-06-15T19:00:00Z", "Group")]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.TeamPlaysItself], result.ValidationErrors["matches[0].awayCode"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Fact]
    public async Task Import_DuplicateTeamCode_ReturnsValidationErrorAndSavesNothing()
    {
        var request = ValidRequest() with
        {
            Teams =
            [
                new ImportTeamRequest("Equipo A", "EQA"),
                new ImportTeamRequest("Equipo B", "EQB"),
                new ImportTeamRequest("Otro equipo", "eqa")
            ]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.DuplicateTeamCode("EQA")], result.ValidationErrors["teams[2].code"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Fact]
    public async Task Import_DuplicateTeamName_ReturnsValidationErrorAndSavesNothing()
    {
        var request = ValidRequest() with
        {
            Teams =
            [
                new ImportTeamRequest("Equipo A", "EQA"),
                new ImportTeamRequest("Equipo B", "EQB"),
                new ImportTeamRequest("equipo a", "EQC")
            ]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.DuplicateTeamName("equipo a")], result.ValidationErrors["teams[2].name"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Theory]
    [InlineData("2026-06-15T19:00:00")]
    [InlineData("2026-06-15T16:00:00-03:00")]
    [InlineData("15/06/2026 19:00")]
    [InlineData("06/15/2026 19:00:00Z")]
    [InlineData("2026-06-15 19:00:00Z")]
    [InlineData("15 Jun 2026 19:00:00 +00:00")]
    [InlineData("2026-06-15T19:00Z")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Import_KickoffNotInUtc_ReturnsValidationErrorAndSavesNothing(string? kickoff)
    {
        var request = ValidRequest() with
        {
            Matches = [new ImportMatchRequest("EQA", "EQB", kickoff, "Group")]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.KickoffNotUtc], result.ValidationErrors["matches[0].kickoffUtc"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Theory]
    [InlineData("2026-06-15T19:00:00Z")]
    [InlineData("2026-06-15T19:00:00.000Z")]
    [InlineData("2026-06-15T19:00:00+00:00")]
    [InlineData("2026-06-15T19:00:00.0000000+00:00")]
    public async Task Import_KickoffInIsoUtc_IsAccepted(string kickoff)
    {
        var request = ValidRequest() with
        {
            Matches = [new ImportMatchRequest("EQA", "EQB", kickoff, "Group")]
        };

        var result = await _service.ImportAsync(request);

        Assert.True(result.Succeeded);
        var match = _repository.Tournaments.Single().Matches.Single();
        Assert.Equal(new DateTime(2026, 6, 15, 19, 0, 0, DateTimeKind.Utc), match.KickoffUtc);
        Assert.Equal(DateTimeKind.Utc, match.KickoffUtc.Kind);
    }

    [Fact]
    public async Task Import_NamesAtMaxLength_AreAccepted()
    {
        var request = ValidRequest() with
        {
            Name = new string('T', TournamentService.NameMaxLength),
            Teams = [new ImportTeamRequest(new string('A', TournamentService.NameMaxLength), "EQA"), new ImportTeamRequest("Equipo B", "EQB")],
            Matches = [new ImportMatchRequest("EQA", "EQB", "2026-06-15T19:00:00Z", "Group")]
        };

        var result = await _service.ImportAsync(request);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Import_TournamentNameTooLong_ReturnsValidationErrorAndSavesNothing()
    {
        var request = ValidRequest() with { Name = new string('T', TournamentService.NameMaxLength + 1) };

        var result = await _service.ImportAsync(request);

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.NameTooLong], result.ValidationErrors["name"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Fact]
    public async Task Import_TeamNameTooLong_ReturnsValidationErrorAndSavesNothing()
    {
        var request = ValidRequest() with
        {
            Teams =
            [
                new ImportTeamRequest("Equipo A", "EQA"),
                new ImportTeamRequest("Equipo B", "EQB"),
                new ImportTeamRequest(new string('C', TournamentService.NameMaxLength + 1), "EQC")
            ]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.TeamNameTooLong], result.ValidationErrors["teams[2].name"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Theory]
    [InlineData("Final")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("1")]
    public async Task Import_InvalidStage_ReturnsValidationErrorAndSavesNothing(string? stage)
    {
        var request = ValidRequest() with
        {
            Matches = [new ImportMatchRequest("EQA", "EQB", "2026-06-15T19:00:00Z", stage)]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal([TournamentMessages.StageInvalid], result.ValidationErrors["matches[0].stage"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("AB")]
    [InlineData("ABCD")]
    [InlineData("A1B")]
    public async Task Import_InvalidTeamCode_ReturnsValidationErrorAndSavesNothing(string? code)
    {
        var request = ValidRequest() with
        {
            Teams = [new ImportTeamRequest("Equipo A", "EQA"), new ImportTeamRequest("Equipo B", "EQB"), new ImportTeamRequest("Equipo X", code)],
            Matches = [new ImportMatchRequest("EQA", "EQB", "2026-06-15T19:00:00Z", "Group")]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal([TournamentMessages.TeamCodeInvalid], result.ValidationErrors["teams[2].code"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Fact]
    public async Task Import_MissingNameTeamsAndMatches_ReturnsAllErrors()
    {
        var result = await _service.ImportAsync(new ImportTournamentRequest("  ", null, []));

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.NameRequired], result.ValidationErrors["name"]);
        Assert.Equal([TournamentMessages.TeamsRequired], result.ValidationErrors["teams"]);
        Assert.Equal([TournamentMessages.MatchesRequired], result.ValidationErrors["matches"]);
        Assert.Empty(_repository.Tournaments);
    }

    [Fact]
    public async Task Import_NullTeamAndMatchEntries_ReturnsValidationErrors()
    {
        var request = ValidRequest() with
        {
            Teams = [new ImportTeamRequest("Equipo A", "EQA"), new ImportTeamRequest("Equipo B", "EQB"), null],
            Matches = [null]
        };

        var result = await _service.ImportAsync(request);

        Assert.Equal([TournamentMessages.TeamNameRequired], result.ValidationErrors["teams[2].name"]);
        Assert.Equal([TournamentMessages.TeamCodeInvalid], result.ValidationErrors["teams[2].code"]);
        Assert.Contains("matches[0].homeCode", result.ValidationErrors.Keys);
        Assert.Empty(_repository.Tournaments);
    }

    [Fact]
    public async Task List_ReturnsStoredTournaments()
    {
        await _service.ImportAsync(ValidRequest());

        var tournaments = await _service.ListAsync();

        var tournament = Assert.Single(tournaments);
        Assert.Equal("Copa de Prueba", tournament.Name);
        Assert.Equal("Upcoming", tournament.Status);
    }

    [Fact]
    public async Task GetMatches_UnknownTournament_ReturnsNotFound()
    {
        var result = await _service.GetMatchesAsync(123, status: null);

        Assert.Equal(TournamentError.NotFound, result.Error);
        Assert.Equal(TournamentMessages.TournamentNotFound, result.Message);
    }

    [Fact]
    public async Task GetMatches_InvalidStatus_ReturnsValidationError()
    {
        var imported = await _service.ImportAsync(ValidRequest());

        var result = await _service.GetMatchesAsync(imported.Value!.Id, status: "Jugando");

        Assert.Equal(TournamentError.Validation, result.Error);
        Assert.Equal([TournamentMessages.MatchStatusInvalid], result.ValidationErrors["status"]);
    }

    [Fact]
    public async Task GetMatches_PassesStatusFilterAndMapsDtos()
    {
        var imported = await _service.ImportAsync(ValidRequest());

        var result = await _service.GetMatchesAsync(imported.Value!.Id, status: "scheduled");

        Assert.True(result.Succeeded);
        Assert.Equal(MatchStatus.Scheduled, _repository.LastStatusFilter);
        var first = result.Value!.First();
        Assert.Equal("EQA", first.HomeTeam.Code);
        Assert.Equal("Equipo B", first.AwayTeam.Name);
        Assert.Equal("Group", first.Stage);
        Assert.Equal("Scheduled", first.Status);
    }

    // Repositorio en memoria: asigna ids al guardar y aplica el filtro y el orden igual que la base.
    private sealed class FakeTournamentRepository : ITournamentRepository
    {
        public List<Tournament> Tournaments { get; } = [];
        public List<Team> StoredTeams { get; } = [];
        public MatchStatus? LastStatusFilter { get; private set; }

        // Contadores del repositorio, igual que una identidad en la base: los ids no se repiten entre torneos.
        private int _nextTeamId = 1000;
        private int _nextMatchId = 1;

        public Task<IReadOnlyList<Team>> GetTeamsByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Team>>(StoredTeams.Where(t => codes.Contains(t.Code)).ToList());

        public Task AddAsync(Tournament tournament, CancellationToken cancellationToken)
        {
            tournament.Id = Tournaments.Count + 1;
            foreach (var team in tournament.Teams.Select(tt => tt.Team).Where(t => t.Id == 0))
                team.Id = _nextTeamId++;
            foreach (var match in tournament.Matches)
                match.Id = _nextMatchId++;
            Tournaments.Add(tournament);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Tournament>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Tournament>>(Tournaments.ToList());

        public Task<bool> ExistsAsync(int tournamentId, CancellationToken cancellationToken) =>
            Task.FromResult(Tournaments.Any(t => t.Id == tournamentId));

        public Task<IReadOnlyList<Match>> GetMatchesAsync(int tournamentId, MatchStatus? status, CancellationToken cancellationToken)
        {
            LastStatusFilter = status;
            var matches = Tournaments.Single(t => t.Id == tournamentId).Matches
                .Where(m => status is null || m.Status == status)
                .OrderBy(m => m.KickoffUtc)
                .ToList();
            return Task.FromResult<IReadOnlyList<Match>>(matches);
        }
    }
}
