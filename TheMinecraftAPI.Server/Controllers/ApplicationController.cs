// LFInteractive LLC. 2021-2024

using Microsoft.AspNetCore.Mvc;
using TheMinecraftAPI.Platforms.Clients;
using TheMinecraftAPI.Server.Data;

namespace TheMinecraftAPI.Server.Controllers;

[Produces("application/json")]
[Route("/")]
[ApiController]
public class ApplicationController : ControllerBase
{
    [HttpGet()]
    public IActionResult Get()
    {
        return Ok(ApplicationData.GenerateApplicationData());
    }

    [HttpGet("fuzzy-test")]
    public IActionResult FuzzyTest(string a, string b)
    {
        return Ok(UniversalClient.CalculateLevenshteinDifference(a, b));
    }
}