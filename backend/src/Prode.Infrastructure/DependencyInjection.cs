using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prode.Application.Auth;
using Prode.Infrastructure.Authentication;
using Prode.Infrastructure.Persistence;
using Prode.Infrastructure.Persistence.Repositories;
using IPasswordHasher = Prode.Application.Auth.IPasswordHasher;

namespace Prode.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    // Registra los servicios de infraestructura: base de datos, repositorios y autenticación.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // La cadena real se configura con user-secrets o variables de entorno; nunca en el repositorio.
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        services.AddDbContext<ProdeDbContext>(options => options.UseSqlServer(connectionString));

        services.TryAddSingleton(TimeProvider.System);

        services.AddOptions<AdminOptions>()
            .Bind(configuration.GetSection(AdminOptions.SectionName));

        // Falla al arrancar si falta el emisor, la audiencia o la clave de firma.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => o.IsValid(),
                $"Configuración '{JwtOptions.SectionName}' inválida: Issuer y Audience son obligatorios y SigningKey debe tener al menos {JwtOptions.MinSigningKeyLength} caracteres.")
            .ValidateOnStart();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
