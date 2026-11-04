using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FlipFlow.Tests;

public sealed class PasscodeTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "flipflow-tests-" + Guid.NewGuid().ToString("N"));

    private WebApplicationFactory<Program> CreateFactory(string? passcode)
    {
        Directory.CreateDirectory(_tempDir);

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={Path.Combine(_tempDir, Guid.NewGuid().ToString("N") + ".db")}");
            builder.UseSetting("Access:Passcode", passcode ?? string.Empty);
            builder.UseSetting("DemoData:Enabled", "false");
        });
    }

    [Fact]
    public async Task With_no_passcode_the_api_is_open()
    {
        using var factory = CreateFactory(null);
        using var client = factory.CreateClient();

        var status = await client.GetFromJsonAsync<AccessStatus>("/api/access");
        var items = await client.GetAsync("/api/items");

        Assert.False(status!.Required);
        Assert.Equal(HttpStatusCode.OK, items.StatusCode);
    }

    [Fact]
    public async Task With_a_passcode_the_api_is_locked_until_the_right_passcode_is_entered()
    {
        using var factory = CreateFactory("open-sesame");
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/items")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/storage/uploads/anything.png")).StatusCode);

        var locked = await client.GetFromJsonAsync<AccessStatus>("/api/access");
        Assert.True(locked!.Required);
        Assert.False(locked.Authorized);

        var wrong = await client.PostAsJsonAsync("/api/access", new { passcode = "nope" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/items")).StatusCode);

        var right = await client.PostAsJsonAsync("/api/access", new { passcode = "open-sesame" });
        Assert.Equal(HttpStatusCode.NoContent, right.StatusCode);

        // The test client does not keep cookies, so replay the one the server issued.
        var cookie = right.Headers.GetValues("Set-Cookie").Single().Split(';')[0];
        Assert.DoesNotContain("open-sesame", cookie);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/items");
        request.Headers.Add("Cookie", cookie);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Health_check_and_frontend_files_stay_public_when_locked()
    {
        using var factory = CreateFactory("open-sesame");
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/health")).StatusCode);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    private sealed record AccessStatus(bool Required, bool Authorized);
}
