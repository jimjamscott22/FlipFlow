using System.Security.Cryptography;
using System.Text;

namespace FlipFlow.Api.Security;

/// <summary>
/// Optional single shared passcode. If <c>Access:Passcode</c> is empty the feature is off
/// and everything is open (the app is meant for a trusted home network).
/// </summary>
public sealed class AccessPasscodeService
{
    public const string CookieName = "flipflow_access";

    private readonly byte[]? _expectedToken;

    public AccessPasscodeService(IConfiguration configuration)
    {
        var passcode = configuration["Access:Passcode"];

        if (!string.IsNullOrWhiteSpace(passcode))
        {
            _expectedToken = ComputeToken(passcode);
        }
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

    private static byte[] ComputeToken(string passcode)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes($"flipflow-access-v1:{passcode}"));
    }
}
