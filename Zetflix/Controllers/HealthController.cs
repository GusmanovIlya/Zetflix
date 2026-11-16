using Microsoft.AspNetCore.Mvc;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "Zetflix",
            timestampUtc = DateTime.UtcNow
        });
    }
}
