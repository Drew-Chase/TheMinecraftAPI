// LFInteractive LLC. 2021-2024

using Microsoft.AspNetCore.Mvc;
using TheMinecraftAPI.Platforms.Clients;
using TheMinecraftAPI.Server.Data;

namespace TheMinecraftAPI.Server.Controllers;

/// <summary>
/// The controller for managing the application.
/// </summary>
[Produces("application/json")]
[Route("/")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ApplicationController : ControllerBase
{
    /// <summary>
    /// Redirects to the Swagger UI page.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> object that redirects to the Swagger UI page.</returns>
    [HttpGet()]
    public IActionResult Get()
    {
        return Redirect("swagger/ui");
    }
}