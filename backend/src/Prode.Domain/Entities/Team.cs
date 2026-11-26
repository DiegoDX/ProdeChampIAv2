namespace Prode.Domain.Entities;

// Selección o equipo. Se asocia a los torneos mediante TournamentTeam.
public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Código de 3 letras, por ejemplo ARG.
    public string Code { get; set; } = string.Empty;

    public ICollection<TournamentTeam> Tournaments { get; set; } = new List<TournamentTeam>();
}
