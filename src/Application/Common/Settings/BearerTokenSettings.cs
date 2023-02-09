namespace Application.Common.Settings;

public class BearerTokenSettings
{
    public string Secret { set; get; } = default!;

    public string Issuer { set; get; } = default!;

    public string Audiance { set; get; } = default!;

    public int AccessTokenExpirationMinutes { set; get; }

    public int RefreshTokenExpirationHours { set; get; }

    public bool AllowMultipleLoginsFromTheSameUser { set; get; }

    public bool AllowSignoutAllUserActiveClients { set; get; }
}