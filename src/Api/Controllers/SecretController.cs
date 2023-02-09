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

    [Authorize(Policy = nameof(Roles.Admin))]
    [HttpGet("admin")]
    public IActionResult AdminSecret()
    {
        return Ok("You can access admin secret!");
    }
}