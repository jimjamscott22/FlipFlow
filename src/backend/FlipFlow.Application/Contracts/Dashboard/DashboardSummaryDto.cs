namespace FlipFlow.Application.Contracts.Dashboard;

public sealed class DashboardSummaryDto
{
    public int TotalItems { get; set; }

    public int DraftItems { get; set; }

    public int ActiveListings { get; set; }
}
