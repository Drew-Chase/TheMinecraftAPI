// LFInteractive LLC. 2021-2024

using Microsoft.AspNetCore.Mvc;
using TheMinecraftAPI.Server.Data;

namespace TheMinecraftAPI.Server.Controllers;

[Produces("application/json")]
[Route("/api")]
[ApiController]
public class ApplicationController : ControllerBase
{
    [HttpGet()]
    public IActionResult Get()
    {
        return Ok(ApplicationData.GenerateApplicationData());
    }
}