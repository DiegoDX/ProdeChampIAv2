using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Persistence.Configurations;

public class TournamentTeamConfiguration : IEntityTypeConfiguration<TournamentTeam>
{
    public void Configure(EntityTypeBuilder<TournamentTeam> builder)
    {
        builder.ToTable("TournamentTeams");
        builder.HasKey(tt => new { tt.TournamentId, tt.TeamId });

        builder.Property(tt => tt.GroupName).HasMaxLength(10);

        builder.HasOne(tt => tt.Tournament)
            .WithMany(t => t.Teams)
            .HasForeignKey(tt => tt.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tt => tt.Team)
            .WithMany(t => t.Tournaments)
            .HasForeignKey(tt => tt.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
