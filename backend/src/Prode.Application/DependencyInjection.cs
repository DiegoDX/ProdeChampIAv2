using Microsoft.Extensions.DependencyInjection;
using Prode.Application.Auth;
using Prode.Application.Tournaments;

namespace Prode.Application;

public static class DependencyInjection
{
    // Registra los casos de uso de la capa de aplicación.
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<TournamentService>();
        return services;
    }
}
