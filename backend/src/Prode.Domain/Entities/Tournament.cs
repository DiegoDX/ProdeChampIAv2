using Prode.Domain.Enums;

namespace Prode.Domain.Entities;

// Torneo (Mundial, Copa América, etc.).
public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TournamentStatus Status { get; set; }

    // Se completan al terminar el torneo.
    public int? ChampionTeamId { get; set; }
    public Team? ChampionTeam { get; set; }
    public string? TopScorerName { get; set; }

    public ICollection<TournamentTeam> Teams { get; set; } = new List<TournamentTeam>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
