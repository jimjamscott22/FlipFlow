namespace FlipFlow.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "FlipFlow";

    public string Audience { get; set; } = "FlipFlow.Client";

    public string SigningKey { get; set; } = "development-signing-key-change-me";

    public int ExpirationMinutes { get; set; } = 120;
}
