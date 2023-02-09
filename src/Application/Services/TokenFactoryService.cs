using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class TokenFactoryService : ITokenFactoryService
{
    private readonly ILogger<TokenFactoryService> _logger;
    private readonly ISecurityService _securityService;
    private readonly BearerTokenSettings _tokenSettings;
    private readonly UserManager<User> _userManager;

    public TokenFactoryService(UserManager<User> userManager, ISecurityService securityService,
        IOptions<BearerTokenSettings> tokenSettings, ILogger<TokenFactoryService> logger)
    {
        _securityService = securityService;
        _userManager = userManager;
        _tokenSettings = tokenSettings.Value;
        _logger = logger;
    }

    public async Task<JwtTokenResponse> CreateJwtTokenAsync(User user)
    {
        var (accessToken, claims) = await CreateAccessTokenAsync(user);

        var (refreshTokenValue, refreshTokenSerial) = CreateRefreshToken();

        return new JwtTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            RefreshTokenSerial = refreshTokenSerial,
            Claims = claims
        };
    }

    public string? GetRefreshTokenSerial(string refreshTokenValue)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
            return string.Empty;

        var decodedRefreshTokenPrincipal = new ClaimsPrincipal();
        try
        {
            decodedRefreshTokenPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                refreshTokenValue,
                new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Secret)),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out _
            );
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to validate RefreshToken Value : {refreshTokenValue}. ERROR : {ex}", ex,
                refreshTokenValue);
        }

        return decodedRefreshTokenPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value;
    }

    private async Task<(string AccessToken, List<Claim> Claims)> CreateAccessTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            // Unique Id for all Jwt tokes
            new(JwtRegisteredClaimNames.Jti, _securityService.CreateCryptographicallySecureGuid().ToString(),
                ClaimValueTypes.String, _tokenSettings.Issuer),
            // Issuer
            new(JwtRegisteredClaimNames.Iss, _tokenSettings.Issuer, ClaimValueTypes.String, _tokenSettings.Issuer),
            // Issued at
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64, _tokenSettings.Issuer),
            new(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String, _tokenSettings.Issuer),
            new(ClaimTypes.Name, user.UserName, ClaimValueTypes.String, _tokenSettings.Issuer),
            new(ClaimTypes.Email, user.Email, ClaimValueTypes.String, _tokenSettings.Issuer),
            new(ClaimTypes.SerialNumber, user.SerialNumber, ClaimValueTypes.String, _tokenSettings.Issuer),
            new(ClaimTypes.UserData, user.Id.ToString(), ClaimValueTypes.String, _tokenSettings.Issuer)
        };

        // add roles
        var roles = await _userManager.GetRolesAsync(user);
        foreach (string? role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String, _tokenSettings.Issuer));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _tokenSettings.Issuer,
            audience: _tokenSettings.Audiance,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_tokenSettings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (accessToken, claims);
    }

    private (string RefreshTokenValue, string RefreshTokenSerial) CreateRefreshToken()
    {
        string refreshTokenSerial = _securityService.CreateCryptographicallySecureGuid().ToString().Replace("-", "");

        var claims = new List<Claim>
        {
            // Unique Id for all Jwt tokes
            new(JwtRegisteredClaimNames.Jti, _securityService.CreateCryptographicallySecureGuid().ToString(),
                ClaimValueTypes.String, _tokenSettings.Issuer),
            // Issuer
            new(JwtRegisteredClaimNames.Iss, _tokenSettings.Issuer, ClaimValueTypes.String, _tokenSettings.Issuer),
            // Issued at
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64, _tokenSettings.Issuer),
            // for invalidation
            new(ClaimTypes.SerialNumber, refreshTokenSerial, ClaimValueTypes.String, _tokenSettings.Issuer)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: _tokenSettings.Issuer,
            audience: _tokenSettings.Audiance,
            claims: claims,
            notBefore: now,
            expires: now.AddHours(_tokenSettings.RefreshTokenExpirationHours),
            signingCredentials: creds);

        string refreshTokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return (refreshTokenValue, refreshTokenSerial);
    }
}