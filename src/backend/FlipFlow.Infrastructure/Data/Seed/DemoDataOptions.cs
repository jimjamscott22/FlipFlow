namespace FlipFlow.Infrastructure.Data.Seed;

public sealed class DemoDataOptions
{
    public const string SectionName = "DemoData";

    /// <summary>When true, a few sample items are added on startup if the database is empty.</summary>
    public bool Enabled { get; set; }
}
