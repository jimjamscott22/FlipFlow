namespace FlipFlow.Application.Contracts.Dashboard;

public sealed class DashboardSummaryDto
{
    public string DisplayName { get; set; } = string.Empty;

    public int TotalItems { get; set; }

    public int DraftItems { get; set; }

    public int ActiveListings { get; set; }

    public int PendingRecommendations { get; set; }
}
