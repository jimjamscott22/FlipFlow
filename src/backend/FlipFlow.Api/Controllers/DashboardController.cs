using FlipFlow.Application.Abstractions.Auth;
using FlipFlow.Application.Abstractions.Common;
using FlipFlow.Application.Contracts.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlipFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(
    ICurrentUserService currentUserService,
    IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> Summary(CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        var summary = await dashboardService.GetSummaryAsync(userId, cancellationToken);

        return Ok(summary);
    }
}
