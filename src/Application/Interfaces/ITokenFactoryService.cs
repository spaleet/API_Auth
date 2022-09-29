namespace Application.Interfaces;

public interface ITokenFactoryService
{
    Task<JwtTokenResponse> CreateJwtTokenAsync(User user);

    string GetRefreshTokenSerial(string refreshTokenValue);
}
