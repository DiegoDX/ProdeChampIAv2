using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prode.Infrastructure.Persistence;

namespace Prode.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    // Registra los servicios de infraestructura (base de datos y, más adelante, implementaciones de repositorios).
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // La cadena real se configura con user-secrets o variables de entorno; nunca en el repositorio.
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        services.AddDbContext<ProdeDbContext>(options => options.UseSqlServer(connectionString));

        return services;
    }
}
