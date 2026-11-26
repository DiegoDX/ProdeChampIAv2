using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Prode.Domain.Entities;
using Prode.Infrastructure.Persistence;

namespace Prode.UnitTests.Infrastructure;

// Verifica el modelo de EF Core sin conectarse a la base de datos.
public class ProdeModelTests
{
    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<ProdeDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;

        using var context = new ProdeDbContext(options);
        return context.Model;
    }

    [Theory]
    [InlineData(typeof(User), new[] { nameof(User.Email) })]
    [InlineData(typeof(League), new[] { nameof(League.InviteCode) })]
    [InlineData(typeof(Prediction), new[] { nameof(Prediction.LeagueId), nameof(Prediction.UserId), nameof(Prediction.MatchId) })]
    [InlineData(typeof(SpecialPrediction), new[] { nameof(SpecialPrediction.LeagueId), nameof(SpecialPrediction.UserId), nameof(SpecialPrediction.TournamentId) })]
    public void Model_HasUniqueIndex(Type entityType, string[] properties)
    {
        var entity = BuildModel().FindEntityType(entityType)!;

        var index = entity.GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(properties));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Theory]
    [InlineData(typeof(TournamentTeam), new[] { nameof(TournamentTeam.TournamentId), nameof(TournamentTeam.TeamId) })]
    [InlineData(typeof(LeagueTournament), new[] { nameof(LeagueTournament.LeagueId), nameof(LeagueTournament.TournamentId) })]
    [InlineData(typeof(LeagueMember), new[] { nameof(LeagueMember.LeagueId), nameof(LeagueMember.UserId) })]
    public void Model_HasCompositePrimaryKey(Type entityType, string[] properties)
    {
        var key = BuildModel().FindEntityType(entityType)!.FindPrimaryKey()!;

        Assert.Equal(properties, key.Properties.Select(p => p.Name));
    }

    [Fact]
    public void Model_AllRelationshipsUseRestrictDelete()
    {
        var foreignKeys = BuildModel().GetEntityTypes().SelectMany(e => e.GetForeignKeys()).ToList();

        Assert.NotEmpty(foreignKeys);
        Assert.All(foreignKeys, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void Model_AllDateTimePropertiesUseUtcConverter()
    {
        var dateProperties = BuildModel().GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?))
            .ToList();

        Assert.NotEmpty(dateProperties);
        Assert.All(dateProperties, p => Assert.IsType<UtcDateTimeConverter>(p.GetValueConverter()));
    }
}
