namespace Application.Interfaces;

public interface ITokenStoreService
{
    Task AddUserToken(AuthToken userToken);

    Task AddUserToken(User user, string refreshTokenSerial, string accessToken, string refreshTokenSourceSerial = null);

    Task<bool> IsValidToken(string accessToken, Guid userId);

    Task DeleteExpiredTokens();

    Task<AuthToken?> FindToken(string refreshTokenValue);

    Task DeleteToken(string refreshTokenValue);

    Task DeleteTokensWithSameRefreshTokenSource(string refreshTokenIdHashSource);

    Task InvalidateUserTokens(Guid userId);

    Task RevokeUserBearerTokens(Guid userIdValue, string refreshTokenValue);
}