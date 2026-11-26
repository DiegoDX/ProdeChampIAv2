using Prode.Application.Tournaments;

namespace Prode.Api.Endpoints;

// Endpoints de torneos. Solo traducen los resultados de TournamentService a respuestas HTTP.
public static class TournamentEndpoints
{
    public static IEndpointRouteBuilder MapTournamentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/tournaments/import", async (ImportTournamentRequest request, TournamentService service, CancellationToken ct) =>
            {
                var result = await service.ImportAsync(request, ct);
                return result.Succeeded
                    ? Results.Created($"/tournaments/{result.Value!.Id}/matches", result.Value)
                    : ToErrorResult(result);
            })
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithTags("Admin")
            .WithName("ImportTournament")
            .Produces<ImportTournamentResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        var group = app.MapGroup("/tournaments")
            .RequireAuthorization()
            .WithTags("Tournaments");

        group.MapGet("/", async (TournamentService service, CancellationToken ct) =>
                Results.Ok(await service.ListAsync(ct)))
            .WithName("ListTournaments")
            .Produces<IReadOnlyList<TournamentDto>>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:int}/matches", async (int id, string? status, TournamentService service, CancellationToken ct) =>
            {
                var result = await service.GetMatchesAsync(id, status, ct);
                return result.Succeeded ? Results.Ok(result.Value) : ToErrorResult(result);
            })
            .WithName("ListTournamentMatches")
            .Produces<IReadOnlyList<MatchDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static IResult ToErrorResult<T>(TournamentResult<T> result) => result.Error switch
    {
        TournamentError.Validation => Results.ValidationProblem(
            result.ValidationErrors.ToDictionary(e => e.Key, e => e.Value),
            title: result.Message),
        TournamentError.NotFound => Results.Problem(
            title: result.Message, statusCode: StatusCodes.Status404NotFound),
        _ => throw new InvalidOperationException($"Error de torneo no contemplado: {result.Error}.")
    };
}
