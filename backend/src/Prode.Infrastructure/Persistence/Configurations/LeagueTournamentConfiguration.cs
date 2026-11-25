using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Persistence.Configurations;

public class LeagueTournamentConfiguration : IEntityTypeConfiguration<LeagueTournament>
{
    public void Configure(EntityTypeBuilder<LeagueTournament> builder)
    {
        builder.ToTable("LeagueTournaments");
        builder.HasKey(lt => new { lt.LeagueId, lt.TournamentId });

        builder.HasOne(lt => lt.League)
            .WithMany(l => l.Tournaments)
            .HasForeignKey(lt => lt.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lt => lt.Tournament)
            .WithMany()
            .HasForeignKey(lt => lt.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
