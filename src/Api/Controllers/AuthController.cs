using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAccountRequest model)
    {
        await _userService.RegisterAsync(model);

        return Ok("User created successfully.");
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromForm] AuthenticateUserRequest login)
    {
        var result = await _userService.AuthenticateUserAsync(login);

        return Ok(result);
    }
}
