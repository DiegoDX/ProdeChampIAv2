using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Prode.Infrastructure.Persistence;

namespace Prode.IntegrationTests;

// Levanta la Api en memoria con SQLite en memoria en lugar de SQL Server, para correr en CI sin base de datos.
public sealed class ProdeApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@prode.test";
    public const string Issuer = "Prode.Tests";
    public const string Audience = "Prode.Tests";

    // Clave solo para tests; no se usa en ningún entorno real.
    public const string SigningKey = "clave-de-prueba-solo-para-tests-0123456789";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Issuer", Issuer);
        builder.UseSetting("Jwt:Audience", Audience);
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.UseSetting("Admin:Email", AdminEmail);

        builder.ConfigureServices(services =>
        {
            // Quita la configuración de SQL Server y registra SQLite sobre una conexión que vive lo que la factory.
            var sqlServerDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ProdeDbContext>)
                            || d.ServiceType == typeof(IDbContextOptionsConfiguration<ProdeDbContext>))
                .ToList();
            foreach (var descriptor in sqlServerDescriptors)
                services.Remove(descriptor);

            _connection.Open();
            services.AddDbContext<ProdeDbContext>(options => options.UseSqlite(_connection));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ProdeDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
