using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class SecretController : BaseController
{
    [Authorize(Policy = nameof(Roles.BasicUser))]
    [HttpGet("user")]
    public IActionResult UserSecret()
    {
        return Ok("You can access user secret!");
    }
}
