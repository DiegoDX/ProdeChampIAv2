using Prode.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Documento OpenAPI y Swagger UI solo en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Prode API v1"));
}

app.UseHttpsRedirection();

// Endpoint de salud: indica que la API está levantada (no verifica la base de datos).
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

app.Run();
