using System.Reflection;
using FlipFlow.Api.Extensions;
using FlipFlow.Api.Middleware;
using FlipFlow.Api.Security;
using FlipFlow.Application;
using FlipFlow.Infrastructure;
using FlipFlow.Infrastructure.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<AccessPasscodeMiddleware>();

// One process serves everything: the built frontend from wwwroot, uploaded photos, and the API.
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseStaticFilesForUploads();

// Routing must come after the static-file middleware. Otherwise the catch-all SPA fallback
// below is matched first and the JS and CSS requests would receive index.html instead.
app.UseRouting();

app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev"
}));

// Client-side routes (/items/123 and so on) fall back to the SPA entry point.
// Unknown /api and /storage paths stay real 404s instead of returning HTML.
app.MapFallback("{*path:regex(^(?!api/|storage/).*$)}", (IWebHostEnvironment environment) =>
{
    var indexFile = environment.WebRootFileProvider.GetFileInfo("index.html");

    return indexFile.Exists
        ? Results.File(indexFile.CreateReadStream(), "text/html; charset=utf-8")
        : Results.Text("The frontend has not been built yet. Run ./run.sh (or .\\run.ps1) to build and start FlipFlow.", statusCode: StatusCodes.Status404NotFound);
});

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>();
    await initializer.InitializeAsync();
}

app.Run();

// Lets the test project host the app with WebApplicationFactory<Program>.
public partial class Program;
