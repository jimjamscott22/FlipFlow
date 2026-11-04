using FlipFlow.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace FlipFlow.Api.Controllers;

[ApiController]
[Route("api/access")]
public sealed class AccessController(AccessPasscodeService passcode) : ControllerBase
{
    public sealed record AccessStatusDto(bool Required, bool Authorized);

    public sealed class AccessRequestDto
    {
        public string Passcode { get; set; } = string.Empty;
    }

    /// <summary>Tells the frontend whether to show the passcode screen.</summary>
    [HttpGet]
    public ActionResult<AccessStatusDto> GetStatus()
    {
        return Ok(new AccessStatusDto(passcode.IsRequired, passcode.IsAuthorized(HttpContext)));
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] AccessRequestDto request, CancellationToken cancellationToken)
    {
        if (!passcode.IsRequired)
        {
            return NoContent();
        }

        if (!passcode.IsPasscodeCorrect(request.Passcode))
        {
            // A short fixed delay makes guessing from another device impractical.
            await Task.Delay(750, cancellationToken);
            return Unauthorized(new { message = "That passcode is not correct." });
        }

        Response.Cookies.Append(AccessPasscodeService.CookieName, passcode.CreateCookieValue(), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        });

        return NoContent();
    }

    [HttpDelete]
    public IActionResult ClearAccess()
    {
        Response.Cookies.Delete(AccessPasscodeService.CookieName);
        return NoContent();
    }
}
