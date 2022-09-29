using System.Text.Json.Serialization;

namespace Application.Common.Models;

public class RegisterAccountRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = default!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;

    [JsonPropertyName("password")]
    public string Password { get; set; } = default!;
}
