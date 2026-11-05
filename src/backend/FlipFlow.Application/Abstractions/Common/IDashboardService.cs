using FlipFlow.Application.Contracts.Dashboard;

namespace FlipFlow.Application.Abstractions.Common;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
