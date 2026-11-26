using Microsoft.EntityFrameworkCore;
using Prode.Application.Tournaments;
using Prode.Domain.Entities;
using Prode.Domain.Enums;

namespace Prode.Infrastructure.Persistence.Repositories;

public sealed class TournamentRepository(ProdeDbContext context) : ITournamentRepository
{
    // Consulta con seguimiento: los equipos devueltos se reutilizan en el torneo que se guarda después.
    public async Task<IReadOnlyList<Team>> GetTeamsByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken) =>
        await context.Teams.Where(t => codes.Contains(t.Code)).ToListAsync(cancellationToken);

    // Un único SaveChanges guarda torneo, equipos nuevos y partidos dentro de la misma transacción.
    public async Task AddAsync(Tournament tournament, CancellationToken cancellationToken)
    {
        context.Tournaments.Add(tournament);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tournament>> ListAsync(CancellationToken cancellationToken) =>
        await context.Tournaments.AsNoTracking().OrderBy(t => t.Id).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int tournamentId, CancellationToken cancellationToken) =>
        context.Tournaments.AnyAsync(t => t.Id == tournamentId, cancellationToken);

    public async Task<IReadOnlyList<Match>> GetMatchesAsync(int tournamentId, MatchStatus? status, CancellationToken cancellationToken)
    {
        var query = context.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.TournamentId == tournamentId);

        if (status is not null)
            query = query.Where(m => m.Status == status);

        return await query
            .OrderBy(m => m.KickoffUtc)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);
    }
}
