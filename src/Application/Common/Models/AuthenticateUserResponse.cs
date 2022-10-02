using System.Text.Json.Serialization;

namespace Application.Common.Models;

public record AuthenticateUserResponse
{
    public AuthenticateUserResponse(JwtTokenResponse token)
    {
        AccessToken = token.AccessToken;
        RefreshToken = token.RefreshToken;
    }

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; }
}
