using Microsoft.AspNetCore.Mvc;

namespace TheMinecraftAPI.Server.Controllers;

[Route("auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    //https://api.theminecraftapi.com/auth/oauth/github?error=&error_description=&error_uri=
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