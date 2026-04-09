using FlipFlow.Application.Abstractions.Common;
using FlipFlow.Application.Contracts.Dashboard;
using FlipFlow.Domain.Enums;
using FlipFlow.Infrastructure.Auth;
using FlipFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlipFlow.Infrastructure.Services;

public sealed class DashboardService(
    AppDbContext dbContext,
    UserManager<AppUser> userManager) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstAsync(x => x.Id == userId, cancellationToken);

        var totalItems = await dbContext.Items.CountAsync(x => x.OwnerUserId == userId, cancellationToken);
        var draftItems = await dbContext.Items.CountAsync(x => x.OwnerUserId == userId && x.Status == ItemStatus.Draft, cancellationToken);
        var activeListings = await dbContext.MarketplaceListings.CountAsync(
            x => x.Item != null &&
                 x.Item.OwnerUserId == userId &&
                 x.Status == MarketplaceListingStatus.Published,
            cancellationToken);
        var pendingRecommendations = await dbContext.RepricingRecommendations.CountAsync(
            x => x.Item != null &&
                 x.Item.OwnerUserId == userId &&
                 !x.IsApplied,
            cancellationToken);

        return new DashboardSummaryDto
        {
            DisplayName = user.DisplayName,
            TotalItems = totalItems,
            DraftItems = draftItems,
            ActiveListings = activeListings,
            PendingRecommendations = pendingRecommendations
        };
    }
}
