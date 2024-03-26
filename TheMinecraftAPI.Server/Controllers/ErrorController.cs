using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace TheMinecraftAPI.Server.Controllers;

/// <summary>
/// Controller that handles error requests and logs the error using Serilog.
/// </summary>
[Route("error")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    /// <summary>
    /// Handles error requests and logs the error using Serilog.
    /// </summary>
    /// <param name="code">The HTTP status code of the error.</param>
    /// <param name="url">The URL of the request that caused the error.</param>
    /// <returns>An IActionResult representing the response to the error request.</returns>
    [HttpGet("{code:int}")]
    public IActionResult Index([FromRoute] int code, [FromQuery] string url)
    {
        Log.Error("Error {code} for {url}", code, url);
        return new ContentResult()
        {
            StatusCode = code,
            ContentType = "application/json",
            Content = $"{{\"error\":{code},\"url\":\"{url}\"}}"
        };
    }
}