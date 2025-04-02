using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetExample.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    public void Get()
    {
    }
}