using Microsoft.AspNetCore.Mvc;

namespace TheMinecraftAPI.Server.Controllers;

/// <summary>
/// The AuthenticationController handles user authentication.
/// </summary>
[Route("auth")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthenticationController : ControllerBase
{
    /// <summary>
    /// Handle OAuth authentication with GitHub.
    /// </summary>
    /// <param name="code">The authorization code returned by GitHub after user authorization.</param>
    /// <param name="state">The optional state parameter.</param>
    /// <param name="error">The optional error message.</param>
    /// <param name="errorDescription">The optional error description.</param>
    /// <param name="errorUri">The optional error URI.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the authentication process.</returns>
    [HttpGet("oauth/github")]
    public IActionResult OAuth([FromQuery] string code, [FromQuery] string? state, [FromQuery] string? error, [FromQuery(Name = "error_description")] string? errorDescription, [FromQuery(Name = "error_uri")] Uri? errorUri)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            Redirect($"https://theminecraftapi.com/auth/login?error={error}&error_description={errorDescription}&error_uri={errorUri}");
        }
        // Add cookie to client with access token


        return Ok();
    }
}