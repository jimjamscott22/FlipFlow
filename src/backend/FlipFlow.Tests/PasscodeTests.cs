using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FlipFlow.Tests;

public sealed class PasscodeTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "flipflow-tests-" + Guid.NewGuid().ToString("N"));

    private WebApplicationFactory<Program> CreateFactory(string? passcode, string? keyPath = null)
    {
        Directory.CreateDirectory(_tempDir);

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={Path.Combine(_tempDir, Guid.NewGuid().ToString("N") + ".db")}");
            builder.UseSetting("Access:Passcode", passcode ?? string.Empty);
            builder.UseSetting("Access:KeyPath", keyPath ?? Path.Combine(_tempDir, "access.key"));
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

    [Theory]
    [InlineData("http://localhost", false)]
    [InlineData("https://localhost", true)]
    public async Task Access_cookie_is_secure_only_when_the_request_is_https(string baseAddress, bool expectSecure)
    {
        using var factory = CreateFactory("open-sesame");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(baseAddress),
            AllowAutoRedirect = false
        });

        var response = await client.PostAsJsonAsync("/api/access", new { passcode = "open-sesame" });
        var setCookie = response.Headers.GetValues("Set-Cookie").Single();

        Assert.Equal(expectSecure, setCookie.Contains("secure", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_cookie_computed_from_a_guessed_passcode_is_rejected()
    {
        // Anyone can compute SHA256 of a guess. The cookie must not be that, or guesses
        // could be tested through GET /api/access without the delay on the submit endpoint.
        using var factory = CreateFactory("open-sesame");
        using var client = factory.CreateClient();
        var forged = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes("flipflow-access-v1:open-sesame")));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/items");
        request.Headers.Add("Cookie", $"flipflow_access={forged}");

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task The_cookie_survives_a_restart_but_not_a_passcode_change()
    {
        var keyPath = Path.Combine(_tempDir, "shared", "access.key");

        async Task<string> SignInAsync(string passcode)
        {
            using var factory = CreateFactory(passcode, keyPath);
            using var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/access", new { passcode });
            return response.Headers.GetValues("Set-Cookie").Single().Split(';')[0];
        }

        async Task<HttpStatusCode> CallWithCookieAsync(string passcode, string cookie)
        {
            using var factory = CreateFactory(passcode, keyPath);
            using var client = factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/items");
            request.Headers.Add("Cookie", cookie);
            return (await client.SendAsync(request)).StatusCode;
        }

        var cookie = await SignInAsync("open-sesame");

        Assert.Equal(HttpStatusCode.OK, await CallWithCookieAsync("open-sesame", cookie));     // restart
        Assert.Equal(HttpStatusCode.Unauthorized, await CallWithCookieAsync("new-passcode", cookie)); // passcode changed
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
