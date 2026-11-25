using Microsoft.EntityFrameworkCore;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Persistence;

// Contexto de EF Core del Prode. La configuración de cada entidad está en Persistence/Configurations.
public class ProdeDbContext(DbContextOptions<ProdeDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TournamentTeam> TournamentTeams => Set<TournamentTeam>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueTournament> LeagueTournaments => Set<LeagueTournament>();
    public DbSet<LeagueMember> LeagueMembers => Set<LeagueMember>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<SpecialPrediction> SpecialPredictions => Set<SpecialPrediction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdeDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Todas las fechas se guardan y se leen en UTC.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
    }
}
