using Microsoft.AspNetCore.Identity;
using Prode.Domain.Entities;
using IPasswordHasher = Prode.Application.Auth.IPasswordHasher;

namespace Prode.Infrastructure.Authentication;

// Usa solo el PasswordHasher<T> de ASP.NET Core Identity (PBKDF2 con sal), sin el sistema Identity completo.
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(User user, string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(user, passwordHash, password) != PasswordVerificationResult.Failed;
}
