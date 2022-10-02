using System.Text.Json.Serialization;

namespace Application.Common.Models;

public record AuthenticateUserRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;

    [JsonPropertyName("password")]
    public string Password { get; set; } = default!;
}
