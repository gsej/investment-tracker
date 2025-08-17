using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    
    [HttpGet("/healthz")]
    public IActionResult GetHealth()
    {
        return Ok("Healthy");
            
    }
}
