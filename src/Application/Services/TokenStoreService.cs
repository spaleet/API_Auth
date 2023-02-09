using Application.Common.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class TokenStoreService : ITokenStoreService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISecurityService _securityService;
    private readonly ITokenFactoryService _tokenFactoryService;
    private readonly BearerTokenSettings _tokenSettings;

    public TokenStoreService(IApplicationDbContext dbContext, ISecurityService securityService,
        IOptions<BearerTokenSettings> tokenSettings, ITokenFactoryService tokenFactoryService)
    {
        _dbContext = dbContext;
        _securityService = securityService;
        _tokenSettings = tokenSettings.Value;
        _tokenFactoryService = tokenFactoryService;
    }

    public async Task AddUserToken(AuthToken userToken)
    {
        if (!_tokenSettings.AllowMultipleLoginsFromTheSameUser)
        {
            await InvalidateUserTokens(userToken.UserId);
        }
        await DeleteTokensWithSameRefreshTokenSource(userToken.RefreshTokenIdHashSource);

        await _dbContext.AuthTokens.AddAsync(userToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddUserToken(User user, string refreshTokenSerial, string accessToken,
        string refreshTokenSourceSerial = null)
    {
        var now = DateTimeOffset.UtcNow;

        string? refreshSourceSerial = string.IsNullOrWhiteSpace(refreshTokenSourceSerial)
            ? null
            : _securityService.GetSha256Hash(refreshTokenSourceSerial);

        var token = new AuthToken
        {
            UserId = user.Id,
            RefreshTokenIdHash = _securityService.GetSha256Hash(refreshTokenSerial),
            RefreshTokenIdHashSource = refreshSourceSerial,
            AccessTokenHash = _securityService.GetSha256Hash(accessToken),
            RefreshTokenExpiresDateTime = now.AddMinutes(_tokenSettings.RefreshTokenExpirationHours),
            AccessTokenExpiresDateTime = now.AddMinutes(_tokenSettings.AccessTokenExpirationMinutes)
        };

        await AddUserToken(token);
    }

    public async Task<bool> IsValidToken(string accessToken, Guid userId)
    {
        string? accessTokenHash = _securityService.GetSha256Hash(accessToken);

        var userToken = await _dbContext.AuthTokens
            .Where(x => x.AccessTokenHash == accessTokenHash && x.UserId == userId)
            .FirstOrDefaultAsync();

        bool isExpired = userToken?.AccessTokenExpiresDateTime >= DateTimeOffset.UtcNow;

        return isExpired;
    }

    public async Task DeleteExpiredTokens()
    {
        var now = DateTimeOffset.UtcNow;

        var expiredTokens = await _dbContext.AuthTokens
            .Where(x => x.RefreshTokenExpiresDateTime < now)
            .ToListAsync();

        for (int i = 0; i < expiredTokens.Count; i++)
        {
            var token = await _dbContext.AuthTokens.FindAsync(expiredTokens[i].Id);
            _dbContext.AuthTokens.Remove(token);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<AuthToken?> FindToken(string refreshTokenValue)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
            return null;

        string? refreshTokenSerial = _tokenFactoryService.GetRefreshTokenSerial(refreshTokenValue);

        if (string.IsNullOrWhiteSpace(refreshTokenSerial))
            return null;

        string? refreshTokenIdHash = _securityService.GetSha256Hash(refreshTokenSerial);

        return await _dbContext.AuthTokens
            .FirstOrDefaultAsync(x => x.RefreshTokenIdHash == refreshTokenIdHash);
    }

    public async Task DeleteToken(string refreshTokenValue)
    {
        var token = await FindToken(refreshTokenValue);

        if (token is not null)
            _dbContext.AuthTokens.Remove(token);

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteTokensWithSameRefreshTokenSource(string refreshTokenIdHashSource)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenIdHashSource))
            return;

        var tokens = await _dbContext.AuthTokens.Where(t => t.RefreshTokenIdHashSource == refreshTokenIdHashSource
                                                            || (t.RefreshTokenIdHash == refreshTokenIdHashSource
                                                                && t.RefreshTokenIdHashSource == null))
            .ToListAsync();

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = await _dbContext.AuthTokens.FindAsync(tokens[i].Id);
            _dbContext.AuthTokens.Remove(token);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task InvalidateUserTokens(Guid userId)
    {
        var tokens = await _dbContext.AuthTokens
            .Where(x => x.UserId == userId)
            .ToListAsync();

        for (int i = 0; i < tokens.Count; i++)
        {
            var tokenModel = await _dbContext.AuthTokens.FindAsync(tokens[i].Id);

            _dbContext.AuthTokens.Remove(tokenModel);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeUserBearerTokens(Guid userIdValue, string refreshTokenValue)
    {
        if (!(userIdValue == Guid.Empty))
        {
            if (_tokenSettings.AllowSignoutAllUserActiveClients)
            {
                await InvalidateUserTokens(userIdValue);
            }
        }

        if (!string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            string? refreshTokenSerial = _tokenFactoryService.GetRefreshTokenSerial(refreshTokenValue);

            if (!string.IsNullOrWhiteSpace(refreshTokenSerial))
            {
                string? refreshTokenIdHashSource = _securityService.GetSha256Hash(refreshTokenSerial);
                await DeleteTokensWithSameRefreshTokenSource(refreshTokenIdHashSource);
            }
        }

        await DeleteExpiredTokens();
    }
}