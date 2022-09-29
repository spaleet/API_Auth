﻿namespace Application.Interfaces;

public interface ITokenStoreService
{
    Task AddUserToken(AuthToken userToken);

    Task AddUserToken(User user, string refreshTokenSerial, string accessToken, string refreshTokenSourceSerial);

    Task<bool> IsValidToken(string accessToken, string userId);

    Task DeleteExpiredTokens();

    Task<AuthToken> FindToken(string refreshTokenValue);

    Task DeleteToken(string refreshTokenValue);

    Task DeleteTokensWithSameRefreshTokenSource(string refreshTokenIdHashSource);

    Task InvalidateUserTokens(string userId);

    Task RevokeUserBearerTokens(string userIdValue, string refreshTokenValue);
}