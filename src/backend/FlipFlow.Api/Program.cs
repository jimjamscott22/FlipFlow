using System.Reflection;
using FlipFlow.Api.Extensions;
using FlipFlow.Api.Middleware;
using FlipFlow.Application;
using FlipFlow.Infrastructure;
using FlipFlow.Infrastructure.Data.Seed;
using Hangfire;

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

app.UseHttpsRedirection();
app.UseCors(ApiServiceCollectionExtensions.DefaultCorsPolicyName);
app.UseStaticFiles();
app.UseStaticFilesForUploads();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev"
}));

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>();
    await initializer.InitializeAsync();
}

RecurringJob.AddOrUpdate(
    "repricing-placeholder",
    () => Console.WriteLine("TODO: wire repricing background job in Phase 5."),
    Cron.Daily);

app.Run();
