using System.Text.Json;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class AuthController : BaseController
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
        var authenticateResult = await _userService.AuthenticateUserAsync(login);

        string result = JsonSerializer.Serialize(authenticateResult, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromForm] RevokeRefreshTokenRequest token)
    {
        var refreshTokenResult = await _userService.RevokeTokenAsync(token);

        string result = JsonSerializer.Serialize(refreshTokenResult, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Ok(result);
    }

    [HttpPost("is-authenticated")]
    public IActionResult IsAuthenticated()
    {
        if (!User.Identity.IsAuthenticated)
            return Unauthorized();

        return Ok("You're Successfully Authorized!");
    }
}
