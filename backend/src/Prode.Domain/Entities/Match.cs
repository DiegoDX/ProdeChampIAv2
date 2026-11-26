using Prode.Domain.Enums;

namespace Prode.Domain.Entities;

// Partido de un torneo. Los goles son null hasta que se carga el resultado.
public class Match
{
    public int Id { get; set; }

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public int HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;

    public int AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public DateTime KickoffUtc { get; set; }
    public MatchStage Stage { get; set; }
    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }
    public MatchStatus Status { get; set; }
}
