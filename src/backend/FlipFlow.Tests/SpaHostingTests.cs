using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FlipFlow.Tests;

/// <summary>
/// The API serves the built frontend. These tests guard the ordering bug where a catch-all SPA
/// fallback swallowed every request, so scripts and styles came back as index.html.
/// </summary>
public sealed class SpaHostingTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "flipflow-spa-" + Guid.NewGuid().ToString("N"));

    private WebApplicationFactory<Program> CreateFactory()
    {
        var webRoot = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(Path.Combine(webRoot, "assets"));
        File.WriteAllText(Path.Combine(webRoot, "index.html"), "<!doctype html><title>spa</title>");
        File.WriteAllText(Path.Combine(webRoot, "assets", "app.js"), "console.log('app');");

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Production, because Development merges the project's real wwwroot (static web assets)
            // into the file provider, which would make this test depend on a local frontend build.
            builder.UseEnvironment("Production");
            builder.UseWebRoot(webRoot);
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={Path.Combine(_tempDir, "test.db")}");
            builder.UseSetting("DemoData:Enabled", "false");
        });
    }

    [Fact]
    public async Task Static_assets_are_served_as_themselves_not_as_index_html()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/assets/app.js");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("javascript", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("console.log", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/dashboard")]
    [InlineData("/items/3f2b9c1e-0000-0000-0000-000000000000")]
    public async Task Client_side_routes_fall_back_to_index_html(string path)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<title>spa</title>", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/api/does-not-exist")]
    [InlineData("/storage/uploads/missing.png")]
    public async Task Unknown_api_and_upload_paths_are_real_404s(string path)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }
}
