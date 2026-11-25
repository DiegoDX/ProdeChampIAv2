using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Persistence.Configurations;

public class LeagueMemberConfiguration : IEntityTypeConfiguration<LeagueMember>
{
    public void Configure(EntityTypeBuilder<LeagueMember> builder)
    {
        builder.ToTable("LeagueMembers");
        builder.HasKey(lm => new { lm.LeagueId, lm.UserId });

        builder.HasOne(lm => lm.League)
            .WithMany(l => l.Members)
            .HasForeignKey(lm => lm.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lm => lm.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(lm => lm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
