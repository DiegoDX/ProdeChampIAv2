using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Prode.Application.Auth;
using Prode.Domain.Entities;
using Prode.Infrastructure.Authentication;

namespace Prode.UnitTests.Application;

// Casos de uso de autenticación con repositorio en memoria y generador de tokens falso.
public class AuthServiceTests
{
    private const string AdminEmail = "admin@prode.test";
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryUserRepository _users = new();

    private AuthService CreateService(string adminEmail = AdminEmail) => new(
        _users,
        new IdentityPasswordHasher(),
        new FakeTokenGenerator(),
        Options.Create(new AdminOptions { Email = adminEmail }),
        new FakeTimeProvider(Now));

    private static RegisterRequest ValidRegister(string email = "ana@prode.test") =>
        new(email, "Ana", "secreto123");

    [Fact]
    public async Task Register_ValidData_ReturnsTokenAndUser()
    {
        var result = await CreateService().RegisterAsync(ValidRegister());

        Assert.True(result.Succeeded);
        Assert.Equal("token-1", result.Response!.Token);
        Assert.Equal(new UserDto(1, "ana@prode.test", "Ana", false), result.Response.User);
    }

    [Fact]
    public async Task Register_StoresHashedPasswordAndCreationDate()
    {
        await CreateService().RegisterAsync(ValidRegister());

        var stored = _users.Single();
        Assert.NotNull(stored.PasswordHash);
        Assert.NotEqual("secreto123", stored.PasswordHash);
        Assert.True(new IdentityPasswordHasher().Verify(stored, stored.PasswordHash!, "secreto123"));
        Assert.Equal(Now.UtcDateTime, stored.CreatedAtUtc);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task Register_NormalizesEmailAndTrimsDisplayName()
    {
        var result = await CreateService().RegisterAsync(new RegisterRequest("  Ana@Prode.TEST ", "  Ana  ", "secreto123"));

        Assert.Equal("ana@prode.test", result.Response!.User.Email);
        Assert.Equal("Ana", result.Response.User.DisplayName);
    }

    [Fact]
    public async Task Register_DuplicateEmail_IgnoringCase_ReturnsEmailAlreadyRegistered()
    {
        var service = CreateService();
        await service.RegisterAsync(ValidRegister("ana@prode.test"));

        var result = await service.RegisterAsync(ValidRegister("ANA@prode.test"));

        Assert.Equal(AuthError.EmailAlreadyRegistered, result.Error);
        Assert.Equal(AuthMessages.EmailAlreadyRegistered, result.Message);
        Assert.Single(_users);
    }

    [Theory]
    [InlineData(AdminEmail)]
    [InlineData("ADMIN@Prode.Test")]
    public async Task Register_WithAdminEmail_CreatesAdmin(string email)
    {
        var result = await CreateService().RegisterAsync(ValidRegister(email));

        Assert.True(result.Response!.User.IsAdmin);
        Assert.True(_users.Single().IsAdmin);
    }

    [Fact]
    public async Task Register_WithoutAdminConfigured_NoUserIsAdmin()
    {
        var result = await CreateService(adminEmail: "").RegisterAsync(ValidRegister());

        Assert.False(result.Response!.User.IsAdmin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sin-arroba.com")]
    [InlineData("ana@dominio")]
    [InlineData("ana @prode.test")]
    [InlineData("ana@@prode.test")]
    public async Task Register_InvalidEmail_ReturnsValidationError(string? email)
    {
        var result = await CreateService().RegisterAsync(new RegisterRequest(email, "Ana", "secreto123"));

        Assert.Equal(AuthError.Validation, result.Error);
        Assert.Equal([AuthMessages.InvalidEmail], result.ValidationErrors["email"]);
        Assert.Empty(_users);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567")]
    public async Task Register_ShortPassword_ReturnsValidationError(string? password)
    {
        var result = await CreateService().RegisterAsync(new RegisterRequest("ana@prode.test", "Ana", password));

        Assert.Equal(AuthError.Validation, result.Error);
        Assert.Equal([AuthMessages.PasswordTooShort], result.ValidationErrors["password"]);
    }

    [Fact]
    public async Task Register_PasswordWithMinimumLength_Succeeds()
    {
        var result = await CreateService().RegisterAsync(new RegisterRequest("ana@prode.test", "Ana", "12345678"));

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("A")]
    [InlineData("   A   ")]
    [InlineData("12345678901234567890123456789012345678901")] // 41 caracteres
    public async Task Register_InvalidDisplayName_ReturnsValidationError(string? displayName)
    {
        var result = await CreateService().RegisterAsync(new RegisterRequest("ana@prode.test", displayName, "secreto123"));

        Assert.Equal(AuthError.Validation, result.Error);
        Assert.Equal([AuthMessages.InvalidDisplayName], result.ValidationErrors["displayName"]);
    }

    [Theory]
    [InlineData("Al")]
    [InlineData("1234567890123456789012345678901234567890")] // 40 caracteres
    public async Task Register_DisplayNameAtLimits_Succeeds(string displayName)
    {
        var result = await CreateService().RegisterAsync(new RegisterRequest("ana@prode.test", displayName, "secreto123"));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndUser()
    {
        var service = CreateService();
        await service.RegisterAsync(ValidRegister());

        var result = await service.LoginAsync(new LoginRequest(" ANA@prode.test", "secreto123"));

        Assert.True(result.Succeeded);
        Assert.Equal(new UserDto(1, "ana@prode.test", "Ana", false), result.Response!.User);
    }

    [Fact]
    public async Task Login_WrongPassword_AndUnknownEmail_ReturnSameError()
    {
        var service = CreateService();
        await service.RegisterAsync(ValidRegister());

        var wrongPassword = await service.LoginAsync(new LoginRequest("ana@prode.test", "otraClave1"));
        var unknownEmail = await service.LoginAsync(new LoginRequest("nadie@prode.test", "secreto123"));

        Assert.Equal(AuthError.InvalidCredentials, wrongPassword.Error);
        Assert.Equal(AuthError.InvalidCredentials, unknownEmail.Error);
        Assert.Equal(AuthMessages.InvalidCredentials, wrongPassword.Message);
        Assert.Equal(wrongPassword.Message, unknownEmail.Message);
    }

    [Fact]
    public async Task Login_DeactivatedUser_ReturnsInvalidCredentials()
    {
        var service = CreateService();
        await service.RegisterAsync(ValidRegister());
        _users.Single().IsActive = false;

        var result = await service.LoginAsync(new LoginRequest("ana@prode.test", "secreto123"));

        Assert.Equal(AuthError.InvalidCredentials, result.Error);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUser_OrNullIfMissing()
    {
        var service = CreateService();
        await service.RegisterAsync(ValidRegister());

        Assert.Equal(new UserDto(1, "ana@prode.test", "Ana", false), await service.GetCurrentUserAsync(1));
        Assert.Null(await service.GetCurrentUserAsync(99));
    }

    private sealed class FakeTokenGenerator : ITokenGenerator
    {
        public string Generate(User user) => $"token-{user.Id}";
    }

    private sealed class InMemoryUserRepository : List<User>, IUserRepository
    {
        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(this.Any(u => u.Email == email));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(this.SingleOrDefault(u => u.Email == email));

        public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(this.SingleOrDefault(u => u.Id == id));

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            user.Id = Count + 1;
            Add(user);
            return Task.CompletedTask;
        }
    }
}
