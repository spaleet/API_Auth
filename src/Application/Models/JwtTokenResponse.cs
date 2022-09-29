using System.Security.Claims;

namespace Application.Models;

public class JwtTokenResponse
{
    public string AccessToken { get; set; } = default!;

    public string RefreshToken { get; set; } = default!;

    public string RefreshTokenSerial { get; set; } = default!;

    public List<Claim> Claims { get; set; } = new();
}
