using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Persistence.Configurations;

public class SpecialPredictionConfiguration : IEntityTypeConfiguration<SpecialPrediction>
{
    public void Configure(EntityTypeBuilder<SpecialPrediction> builder)
    {
        builder.ToTable("SpecialPredictions");
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.TopScorerName).HasMaxLength(100).IsRequired();

        // Un solo pronóstico de campeón y goleador por usuario, liga y torneo.
        builder.HasIndex(sp => new { sp.LeagueId, sp.UserId, sp.TournamentId }).IsUnique();

        builder.HasOne(sp => sp.League)
            .WithMany()
            .HasForeignKey(sp => sp.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.User)
            .WithMany()
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.Tournament)
            .WithMany()
            .HasForeignKey(sp => sp.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.ChampionTeam)
            .WithMany()
            .HasForeignKey(sp => sp.ChampionTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
