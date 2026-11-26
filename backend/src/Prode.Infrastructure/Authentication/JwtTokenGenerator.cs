using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Prode.Application.Auth;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Authentication;

// Emite tokens JWT firmados con HMAC-SHA256 y define cómo se validan.
public sealed class JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenGenerator
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(60);

    // Nombres cortos de claims; la validación no los remapea a los tipos largos de .NET.
    public const string NameClaim = JwtRegisteredClaimNames.Name;
    public const string RoleClaim = "role";

    public string Generate(User user)
    {
        var settings = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(CultureInfo.InvariantCulture),
            [JwtRegisteredClaimNames.Email] = user.Email,
            [NameClaim] = user.DisplayName
        };
        if (user.IsAdmin)
            claims[RoleClaim] = Roles.Admin;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Claims = claims,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(Lifetime),
            SigningCredentials = new SigningCredentials(CreateSigningKey(settings), SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    // Parámetros de validación usados por la Api. Sin tolerancia de reloj: el token vence a los 60 minutos exactos.
    public static TokenValidationParameters CreateValidationParameters(JwtOptions settings) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = settings.Issuer,
        ValidateAudience = true,
        ValidAudience = settings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = CreateSigningKey(settings),
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = NameClaim,
        RoleClaimType = RoleClaim
    };

    private static SymmetricSecurityKey CreateSigningKey(JwtOptions settings) =>
        new(Encoding.UTF8.GetBytes(settings.SigningKey));
}
