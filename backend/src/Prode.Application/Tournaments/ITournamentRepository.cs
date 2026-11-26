using Prode.Domain.Entities;
using Prode.Domain.Enums;

namespace Prode.Application.Tournaments;

// Acceso a torneos, equipos y partidos persistidos.
public interface ITournamentRepository
{
    // Equipos ya guardados cuyo código está en la lista (los códigos se reciben en mayúsculas).
    Task<IReadOnlyList<Team>> GetTeamsByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken);

    // Guarda el torneo con sus equipos y partidos en una sola operación: se guarda todo o nada.
    Task AddAsync(Tournament tournament, CancellationToken cancellationToken);

    Task<IReadOnlyList<Tournament>> ListAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int tournamentId, CancellationToken cancellationToken);

    // Partidos del torneo con sus equipos, ordenados por fecha; opcionalmente filtrados por estado.
    Task<IReadOnlyList<Match>> GetMatchesAsync(int tournamentId, MatchStatus? status, CancellationToken cancellationToken);
}
