using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace TheMinecraftAPI.Server.Controllers;

[Route("error")]
[ApiController]
public class ErrorController : ControllerBase
{
    [HttpGet("{code:int}")]
    public IActionResult Index([FromRoute]int code, [FromQuery] string url)
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