using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Prode.Api;
using Prode.Application.Auth;
using Prode.Domain.Entities;
using Prode.Infrastructure.Authentication;
using Prode.Infrastructure.Persistence;

namespace Prode.IntegrationTests;

// Prueba los endpoints /auth de punta a punta (HTTP, autenticación JWT y base de datos).
// Cada test usa su propia factory, así la base en memoria empieza vacía.
public sealed class AuthEndpointsTests : IDisposable
{
    private const string Password = "secreto123";

    private readonly ProdeApiFactory _factory = new();
    private readonly HttpClient _client;

    public AuthEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private Task<HttpResponseMessage> RegisterAsync(string email = "ana@prode.test", string displayName = "Ana", string password = Password) =>
        _client.PostAsJsonAsync("/auth/register", new { email, displayName, password });

    private Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        _client.PostAsJsonAsync("/auth/login", new { email, password });

    private Task<HttpResponseMessage> GetMeAsync(string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }

    [Fact]
    public async Task Register_ValidData_Returns201WithToken()
    {
        var response = await RegisterAsync(email: "Ana@Prode.Test");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.True(body.User.Id > 0);
        Assert.Equal("ana@prode.test", body.User.Email);
        Assert.Equal("Ana", body.User.DisplayName);
        Assert.False(body.User.IsAdmin);
    }

    [Fact]
    public async Task Register_TokenExpiresIn60Minutes()
    {
        var body = await (await RegisterAsync()).Content.ReadFromJsonAsync<AuthResponse>();

        var token = new JsonWebTokenHandler().ReadJsonWebToken(body!.Token);
        Assert.Equal(TimeSpan.FromMinutes(60), token.ValidTo - token.IssuedAt);
        Assert.Equal(ProdeApiFactory.Issuer, token.Issuer);
        Assert.Contains(ProdeApiFactory.Audience, token.Audiences);
    }

    [Fact]
    public async Task Register_DoesNotStorePlainTextPassword()
    {
        await RegisterAsync();

        using var scope = _factory.Services.CreateScope();
        var user = await scope.ServiceProvider.GetRequiredService<ProdeDbContext>().Users.SingleAsync();
        Assert.NotNull(user.PasswordHash);
        Assert.DoesNotContain(Password, user.PasswordHash);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409WithSpanishMessage()
    {
        await RegisterAsync(email: "ana@prode.test");

        var response = await RegisterAsync(email: "ANA@prode.test");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(AuthMessages.EmailAlreadyRegistered, problem!.Title);
    }

    [Fact]
    public async Task Register_WithAdminEmail_CreatesAdminWithAdminRoleClaim()
    {
        var response = await RegisterAsync(email: ProdeApiFactory.AdminEmail);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.True(body!.User.IsAdmin);
        var token = new JsonWebTokenHandler().ReadJsonWebToken(body.Token);
        Assert.Contains(token.Claims, c => c.Type == JwtTokenGenerator.RoleClaim && c.Value == Roles.Admin);
    }

    [Fact]
    public async Task Register_RegularUser_TokenHasNoAdminRole()
    {
        var body = await (await RegisterAsync()).Content.ReadFromJsonAsync<AuthResponse>();

        var token = new JsonWebTokenHandler().ReadJsonWebToken(body!.Token);
        Assert.DoesNotContain(token.Claims, c => c.Type == JwtTokenGenerator.RoleClaim);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400ValidationProblem()
    {
        var response = await RegisterAsync(email: "no-es-un-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal([AuthMessages.InvalidEmail], problem!.Errors["email"]);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400ValidationProblem()
    {
        var response = await RegisterAsync(password: "1234567");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal([AuthMessages.PasswordTooShort], problem!.Errors["password"]);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        await RegisterAsync();

        var response = await LoginAsync("ANA@prode.test", Password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal("ana@prode.test", body.User.Email);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await RegisterAsync();

        var response = await LoginAsync("ana@prode.test", "otraClave1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(AuthMessages.InvalidCredentials, problem!.Title);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401WithSameMessageAsWrongPassword()
    {
        await RegisterAsync();

        var wrongPassword = await LoginAsync("ana@prode.test", "otraClave1");
        var unknownEmail = await LoginAsync("nadie@prode.test", Password);

        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        var wrongPasswordProblem = await wrongPassword.Content.ReadFromJsonAsync<ProblemDetails>();
        var unknownEmailProblem = await unknownEmail.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(AuthMessages.InvalidCredentials, unknownEmailProblem!.Title);
        Assert.Equal(
            (wrongPasswordProblem!.Status, wrongPasswordProblem.Type, wrongPasswordProblem.Title, wrongPasswordProblem.Detail),
            (unknownEmailProblem.Status, unknownEmailProblem.Type, unknownEmailProblem.Title, unknownEmailProblem.Detail));
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsAuthenticatedUser()
    {
        var registered = await (await RegisterAsync()).Content.ReadFromJsonAsync<AuthResponse>();

        var response = await GetMeAsync(registered!.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(registered.User, await response.Content.ReadFromJsonAsync<UserDto>());
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await GetMeAsync(token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithExpiredToken_Returns401()
    {
        var registered = await (await RegisterAsync()).Content.ReadFromJsonAsync<AuthResponse>();

        // Token emitido hace 61 minutos: venció hace 1 minuto.
        var issuedAt = DateTimeOffset.UtcNow.AddMinutes(-61);
        var expiredToken = CreateTokenGenerator(new FakeTimeProvider(issuedAt))
            .Generate(new User { Id = registered!.User.Id, Email = registered.User.Email, DisplayName = registered.User.DisplayName });

        var response = await GetMeAsync(expiredToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithTokenSignedWithAnotherKey_Returns401()
    {
        var registered = await (await RegisterAsync()).Content.ReadFromJsonAsync<AuthResponse>();
        var forgedToken = CreateTokenGenerator(TimeProvider.System, signingKey: "otra-clave-que-no-es-la-del-servidor-123")
            .Generate(new User { Id = registered!.User.Id, Email = registered.User.Email, DisplayName = "Ana", IsAdmin = true });

        var response = await GetMeAsync(forgedToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminOnlyPolicy_IsRegisteredAndRequiresAdminRole()
    {
        var policyProvider = _factory.Services.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(AuthorizationPolicies.AdminOnly);

        Assert.NotNull(policy);
        var roles = Assert.Single(policy.Requirements.OfType<RolesAuthorizationRequirement>());
        Assert.Equal([Roles.Admin], roles.AllowedRoles);
    }

    private static JwtTokenGenerator CreateTokenGenerator(TimeProvider timeProvider, string signingKey = ProdeApiFactory.SigningKey) =>
        new(Options.Create(new JwtOptions
        {
            Issuer = ProdeApiFactory.Issuer,
            Audience = ProdeApiFactory.Audience,
            SigningKey = signingKey
        }), timeProvider);
}
