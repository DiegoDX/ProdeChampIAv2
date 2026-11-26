using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Prode.Api;
using Prode.Api.Endpoints;
using Prode.Api.OpenApi;
using Prode.Application;
using Prode.Application.Auth;
using Prode.Infrastructure;
using Prode.Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
    options.AddOperationTransformer<BearerSecurityTransformer>();
});
builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Autenticación JWT: los parámetros se leen de la configuración "Jwt" al resolver las opciones.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
    {
        bearer.MapInboundClaims = false;
        bearer.TokenValidationParameters = JwtTokenGenerator.CreateValidationParameters(jwt.Value);
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(Roles.Admin));

var app = builder.Build();

// Documento OpenAPI y Swagger UI solo en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Prode API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoint de salud: indica que la API está levantada (no verifica la base de datos).
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

app.MapAuthEndpoints();
app.MapTournamentEndpoints();

app.Run();
