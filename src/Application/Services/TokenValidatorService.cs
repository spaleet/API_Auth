using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class TokenValidatorService : ITokenValidatorService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenStoreService _tokenStoreService;

    public TokenValidatorService(UserManager<User> userManager, ITokenStoreService tokenStoreService)
    {
        _userManager = userManager;
        _tokenStoreService = tokenStoreService;
    }

    public async Task ValidateAsync(TokenValidatedContext context)
    {
        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

        if (claimsIdentity?.Claims is null || !claimsIdentity.Claims.Any())
        {
            context.Fail("This is not our issued token. It has no claims.");
            return;
        }

        var serialNumberClaim = claimsIdentity.FindFirst(ClaimTypes.SerialNumber);
        if (serialNumberClaim is null)
        {
            context.Fail("This is not our issued token. It has no serial.");
            return;
        }

        var userId = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail("This is not our issued token. It has no user-id.");
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);

        // if (user is null || user.SerialNumber != serialNumberClaim.Value || !user.EmailConfirmed)

        if (user is null || user.SerialNumber != serialNumberClaim.Value)
        {
            context.Fail("This token is expired. Please login again.");
            return;
        }


        if (context.SecurityToken is not JwtSecurityToken accessToken || string.IsNullOrWhiteSpace(accessToken.RawData) ||
            !await _tokenStoreService.IsValidToken(accessToken.RawData, Guid.Parse(userId)))
        {
            context.Fail("This token is not in our database.");
            return;
        }
    }
}
