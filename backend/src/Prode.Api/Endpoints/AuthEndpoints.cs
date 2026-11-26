using System.Globalization;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Prode.Application.Auth;

namespace Prode.Api.Endpoints;

// Endpoints de autenticación. Solo traducen los resultados de AuthService a respuestas HTTP.
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, AuthService auth, CancellationToken ct) =>
                ToHttpResult(await auth.RegisterAsync(request, ct), created: true))
            .WithName("Register")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginRequest request, AuthService auth, CancellationToken ct) =>
                ToHttpResult(await auth.LoginAsync(request, ct), created: false))
            .WithName("Login")
            .Produces<AuthResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", async (ClaimsPrincipal principal, AuthService auth, CancellationToken ct) =>
            {
                var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (!int.TryParse(subject, NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
                    return Results.Unauthorized();

                var user = await auth.GetCurrentUserAsync(userId, ct);
                return user is null ? Results.Unauthorized() : Results.Ok(user);
            })
            .RequireAuthorization()
            .WithName("Me")
            .Produces<UserDto>()
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static IResult ToHttpResult(AuthResult result, bool created) => result.Error switch
    {
        AuthError.None when created => Results.Created("/auth/me", result.Response),
        AuthError.None => Results.Ok(result.Response),
        AuthError.Validation => Results.ValidationProblem(
            result.ValidationErrors.ToDictionary(e => e.Key, e => e.Value),
            title: result.Message),
        AuthError.EmailAlreadyRegistered => Results.Problem(
            title: result.Message, statusCode: StatusCodes.Status409Conflict),
        AuthError.InvalidCredentials => Results.Problem(
            title: result.Message, statusCode: StatusCodes.Status401Unauthorized),
        _ => throw new InvalidOperationException($"Error de autenticación no contemplado: {result.Error}.")
    };
}
