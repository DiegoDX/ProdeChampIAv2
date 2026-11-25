namespace Prode.Domain.Entities;

// Equipo que participa en un torneo (relación N a N).
public class TournamentTeam
{
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    // Grupo del equipo en el torneo (ej. "A"); null si el torneo no tiene grupos.
    public string? GroupName { get; set; }
}
