using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Persistence.Configurations;

public class PredictionConfiguration : IEntityTypeConfiguration<Prediction>
{
    public void Configure(EntityTypeBuilder<Prediction> builder)
    {
        builder.ToTable("Predictions");
        builder.HasKey(p => p.Id);

        // Un solo pronóstico por usuario, liga y partido.
        builder.HasIndex(p => new { p.LeagueId, p.UserId, p.MatchId }).IsUnique();

        builder.HasOne(p => p.League)
            .WithMany()
            .HasForeignKey(p => p.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Match)
            .WithMany()
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
