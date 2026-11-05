using FlipFlow.Application.Abstractions.Common;
using FlipFlow.Application.Contracts.Dashboard;
using FlipFlow.Domain.Enums;
using FlipFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlipFlow.Infrastructure.Services;

public sealed class DashboardService(AppDbContext dbContext) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        return new DashboardSummaryDto
        {
            TotalItems = await dbContext.Items.CountAsync(cancellationToken),
            DraftItems = await dbContext.Items.CountAsync(x => x.Status == ItemStatus.Draft, cancellationToken),
            ActiveListings = await dbContext.MarketplaceListings.CountAsync(
                x => x.Status == MarketplaceListingStatus.Published,
                cancellationToken)
        };
    }
}
