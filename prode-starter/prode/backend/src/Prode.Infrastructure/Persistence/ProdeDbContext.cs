using Microsoft.EntityFrameworkCore;

namespace Prode.Infrastructure.Persistence;

// Contexto de EF Core del Prode. Las entidades y su configuración se agregan en el issue de entidades y migraciones.
public class ProdeDbContext(DbContextOptions<ProdeDbContext> options) : DbContext(options)
{
}
