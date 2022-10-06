using System.Text.Json.Serialization;

namespace Application.Common.Models;

public class RevokeRefreshTokenRequest
{
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = default!;
}
