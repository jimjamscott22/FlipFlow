using System.Security.Cryptography;
using System.Text;

namespace FlipFlow.Api.Security;

/// <summary>
/// Optional single shared passcode. If <c>Access:Passcode</c> is empty the feature is off
/// and everything is open (the app is meant for a trusted home network).
///
/// The cookie value is an HMAC of the passcode under a random secret that only the server
/// holds (stored in <c>Access:KeyPath</c>, default <c>data/access.key</c>). Because the value
/// cannot be computed without that secret, the only way to test a guess is the submit
/// endpoint, which is slowed down. The cookie stays valid across restarts, and changing
/// the passcode invalidates every existing cookie.
/// </summary>
public sealed class AccessPasscodeService
{
    public const string CookieName = "flipflow_access";

    private readonly byte[] _key = [];
    private readonly byte[]? _expectedToken;

    public AccessPasscodeService(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<AccessPasscodeService> logger)
    {
        var passcode = configuration["Access:Passcode"];

        if (string.IsNullOrWhiteSpace(passcode))
        {
            return;
        }

        var keyPath = configuration["Access:KeyPath"] ?? "data/access.key";
        _key = LoadOrCreateKey(Path.GetFullPath(Path.Combine(environment.ContentRootPath, keyPath)), logger);
        _expectedToken = ComputeToken(passcode);
    }

    public bool IsRequired => _expectedToken is not null;

    public bool IsPasscodeCorrect(string? candidate)
    {
        return _expectedToken is not null
            && !string.IsNullOrEmpty(candidate)
            && CryptographicOperations.FixedTimeEquals(ComputeToken(candidate), _expectedToken);
    }

    /// <summary>The cookie value that proves the passcode was entered. Never the passcode itself.</summary>
    public string CreateCookieValue()
    {
        return Convert.ToHexString(_expectedToken ?? throw new InvalidOperationException("No passcode is configured."));
    }

    public bool IsAuthorized(HttpContext context)
    {
        if (_expectedToken is null)
        {
            return true;
        }

        if (!context.Request.Cookies.TryGetValue(CookieName, out var cookie) || string.IsNullOrEmpty(cookie))
        {
            return false;
        }

        try
        {
            return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(cookie), _expectedToken);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private byte[] ComputeToken(string passcode)
    {
        return HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(passcode));
    }

    private static byte[] LoadOrCreateKey(string path, ILogger logger)
    {
        try
        {
            if (File.Exists(path))
            {
                var existing = File.ReadAllBytes(path);

                if (existing.Length >= 32)
                {
                    return existing;
                }
            }

            var created = RandomNumberGenerator.GetBytes(32);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, created);

            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }

            return created;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Still secure, but everyone has to re-enter the passcode after each restart.
            logger.LogWarning(exception, "Could not read or write the access key at {Path}; using a temporary key for this run.", path);
            return RandomNumberGenerator.GetBytes(32);
        }
    }
}
