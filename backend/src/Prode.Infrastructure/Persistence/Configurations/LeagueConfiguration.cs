using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prode.Domain.Entities;

namespace Prode.Infrastructure.Persistence.Configurations;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("Leagues");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();
        builder.Property(l => l.InviteCode).HasMaxLength(20).IsRequired();

        builder.HasIndex(l => l.InviteCode).IsUnique();

        builder.HasOne(l => l.OwnerUser)
            .WithMany()
            .HasForeignKey(l => l.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
