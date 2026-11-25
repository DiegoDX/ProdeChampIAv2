namespace Prode.Domain.Entities;

// Pronóstico de un usuario para un partido dentro de una liga.
public class Prediction
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }

    // Se calcula al cargar el resultado del partido.
    public int? Points { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
