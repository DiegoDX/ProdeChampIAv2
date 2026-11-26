using Microsoft.Extensions.DependencyInjection;
using Prode.Application.Auth;

namespace Prode.Application;

public static class DependencyInjection
{
    // Registra los casos de uso de la capa de aplicación.
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        return services;
    }
}
