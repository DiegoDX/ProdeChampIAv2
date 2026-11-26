namespace Prode.Domain.Entities;

// Torneo que se juega dentro de una liga (relación N a N).
public class LeagueTournament
{
    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public DateTime AddedAtUtc { get; set; }
}
