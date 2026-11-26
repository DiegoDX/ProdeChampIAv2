namespace Prode.Domain.Entities;

// Pronóstico de campeón y goleador de un usuario para un torneo dentro de una liga.
public class SpecialPrediction
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public int ChampionTeamId { get; set; }
    public Team ChampionTeam { get; set; } = null!;

    // Se compara con Tournament.TopScorerName.
    public string TopScorerName { get; set; } = string.Empty;

    // Se calcula al terminar el torneo.
    public int? Points { get; set; }
}
