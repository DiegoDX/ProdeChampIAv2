using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prode.Application.Auth;
using Prode.Application.Tournaments;
using Prode.Domain.Enums;
using Prode.Infrastructure.Persistence;

namespace Prode.IntegrationTests;

// Prueba la importación y la lectura de torneos de punta a punta (HTTP, autorización y base de datos).
// Cada test usa su propia factory, así la base en memoria empieza vacía.
public sealed class TournamentEndpointsTests : IDisposable
{
    private const string Password = "secreto123";
    private static readonly string SampleFile = Path.Combine(AppContext.BaseDirectory, "samples", "sample-tournament.json");

    private readonly ProdeApiFactory _factory = new();
    private readonly HttpClient _client;

    public TournamentEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<string> GetTokenAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new { email, displayName = "Usuario", password = Password });
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.Token;
    }

    // El administrador se registra una sola vez por test; las llamadas siguientes reutilizan su token.
    private string? _adminToken;

    private async Task<string> GetAdminTokenAsync() => _adminToken ??= await GetTokenAsync(ProdeApiFactory.AdminEmail);

    private Task<string> GetUserTokenAsync() => GetTokenAsync("ana@prode.test");

    private Task<HttpResponseMessage> ImportAsync(string json, string? token) =>
        SendAsync(HttpMethod.Post, "/admin/tournaments/import", token, new StringContent(json, Encoding.UTF8, "application/json"));

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string? token, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }

    private async Task<ImportTournamentResponse> ImportSampleAsAdminAsync()
    {
        var response = await ImportAsync(await File.ReadAllTextAsync(SampleFile), await GetAdminTokenAsync());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ImportTournamentResponse>())!;
    }

    private async Task<(int Tournaments, int Teams, int TournamentTeams, int Matches)> CountRowsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProdeDbContext>();
        return (await db.Tournaments.CountAsync(), await db.Teams.CountAsync(), await db.TournamentTeams.CountAsync(), await db.Matches.CountAsync());
    }

    [Fact]
    public async Task Import_SampleFileAsAdmin_Returns201AndDataIsReadable()
    {
        var imported = await ImportSampleAsAdminAsync();

        Assert.Equal("Copa Ficticia 2026", imported.Name);
        Assert.Equal(4, imported.TeamsCount);
        Assert.Equal(6, imported.MatchesCount);
        Assert.Equal((1, 4, 4, 6), await CountRowsAsync());

        var userToken = await GetUserTokenAsync();
        var tournaments = await (await SendAsync(HttpMethod.Get, "/tournaments", userToken)).Content.ReadFromJsonAsync<List<TournamentDto>>();
        Assert.Equal([new TournamentDto(imported.Id, "Copa Ficticia 2026", "Upcoming")], tournaments);

        var matchesResponse = await SendAsync(HttpMethod.Get, $"/tournaments/{imported.Id}/matches", userToken);
        Assert.Equal(HttpStatusCode.OK, matchesResponse.StatusCode);
        var matches = (await matchesResponse.Content.ReadFromJsonAsync<List<MatchDto>>())!;
        Assert.Equal(6, matches.Count);
        Assert.Equal(matches.OrderBy(m => m.KickoffUtc).Select(m => m.Id), matches.Select(m => m.Id));
        Assert.Equal(new DateTime(2026, 6, 15, 19, 0, 0, DateTimeKind.Utc), matches[0].KickoffUtc);
        Assert.Equal("ATL", matches[0].HomeTeam.Code);
        Assert.Equal("Borealia", matches[0].AwayTeam.Name);
        Assert.All(matches, m => Assert.Equal("Scheduled", m.Status));
    }

    // La igualdad de DateTime ignora el Kind, así que se revisa el texto JSON: la fecha leída de la base debe salir con "Z".
    [Fact]
    public async Task GetMatches_SerializesKickoffAsUtc()
    {
        var imported = await ImportSampleAsAdminAsync();

        var response = await SendAsync(HttpMethod.Get, $"/tournaments/{imported.Id}/matches", await GetUserTokenAsync());

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"kickoffUtc\":\"2026-06-15T19:00:00Z\"", json);
    }

    [Fact]
    public async Task Import_AsRegularUser_Returns403AndSavesNothing()
    {
        var response = await ImportAsync(await File.ReadAllTextAsync(SampleFile), await GetUserTokenAsync());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal((0, 0, 0, 0), await CountRowsAsync());
    }

    [Fact]
    public async Task Import_WithoutToken_Returns401()
    {
        var response = await ImportAsync(await File.ReadAllTextAsync(SampleFile), token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Import_InvalidFile_Returns400WithSpanishMessagesAndSavesNothing()
    {
        const string json = """
            {
              "name": "Torneo inválido",
              "teams": [
                { "name": "Equipo A", "code": "EQA" },
                { "name": "Equipo B", "code": "EQB" },
                { "name": "Equipo A bis", "code": "EQA" }
              ],
              "matches": [
                { "homeCode": "EQA", "awayCode": "EQB", "kickoffUtc": "2026-06-15T19:00:00Z", "stage": "Group" },
                { "homeCode": "EQA", "awayCode": "XYZ", "kickoffUtc": "2026-06-16T19:00:00Z", "stage": "Group" },
                { "homeCode": "EQB", "awayCode": "EQB", "kickoffUtc": "2026-06-17T19:00:00-03:00", "stage": "Group" }
              ]
            }
            """;

        var response = await ImportAsync(json, await GetAdminTokenAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        Assert.Equal(TournamentMessages.ValidationFailed, problem.Title);
        Assert.Equal([TournamentMessages.DuplicateTeamCode("EQA")], problem.Errors["teams[2].code"]);
        Assert.Equal([TournamentMessages.UnknownTeamCode("XYZ")], problem.Errors["matches[1].awayCode"]);
        Assert.Equal([TournamentMessages.TeamPlaysItself], problem.Errors["matches[2].awayCode"]);
        Assert.Equal([TournamentMessages.KickoffNotUtc], problem.Errors["matches[2].kickoffUtc"]);
        Assert.Equal((0, 0, 0, 0), await CountRowsAsync());
    }

    [Fact]
    public async Task Import_SameTeamsTwice_ReusesStoredTeams()
    {
        await ImportSampleAsAdminAsync();
        await ImportSampleAsAdminAsync();

        Assert.Equal((2, 4, 8, 12), await CountRowsAsync());
    }

    [Fact]
    public async Task GetMatches_FilterByStatus_ReturnsOnlyMatchingMatches()
    {
        var imported = await ImportSampleAsAdminAsync();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProdeDbContext>();
            var first = await db.Matches.OrderBy(m => m.Id).FirstAsync();
            first.Status = MatchStatus.Finished;
            first.HomeGoals = 2;
            first.AwayGoals = 1;
            await db.SaveChangesAsync();
        }
        var token = await GetUserTokenAsync();

        var finished = await (await SendAsync(HttpMethod.Get, $"/tournaments/{imported.Id}/matches?status=Finished", token))
            .Content.ReadFromJsonAsync<List<MatchDto>>();
        var scheduled = await (await SendAsync(HttpMethod.Get, $"/tournaments/{imported.Id}/matches?status=Scheduled", token))
            .Content.ReadFromJsonAsync<List<MatchDto>>();

        var finishedMatch = Assert.Single(finished!);
        Assert.Equal((2, 1), (finishedMatch.HomeGoals!.Value, finishedMatch.AwayGoals!.Value));
        Assert.Equal(5, scheduled!.Count);
        Assert.All(scheduled, m => Assert.Equal("Scheduled", m.Status));
    }

    [Fact]
    public async Task GetMatches_InvalidStatus_Returns400()
    {
        var imported = await ImportSampleAsAdminAsync();

        var response = await SendAsync(HttpMethod.Get, $"/tournaments/{imported.Id}/matches?status=Jugando", await GetUserTokenAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal([TournamentMessages.MatchStatusInvalid], problem!.Errors["status"]);
    }

    [Fact]
    public async Task GetMatches_UnknownTournament_Returns404()
    {
        var response = await SendAsync(HttpMethod.Get, "/tournaments/999/matches", await GetUserTokenAsync());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(TournamentMessages.TournamentNotFound, problem!.Title);
    }

    [Theory]
    [InlineData("/tournaments")]
    [InlineData("/tournaments/1/matches")]
    public async Task ReadEndpoints_WithoutToken_Return401(string url)
    {
        var response = await SendAsync(HttpMethod.Get, url, token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
