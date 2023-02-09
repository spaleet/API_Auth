namespace Application.Interfaces;

public interface IUserService
{
    Task RegisterAsync(RegisterAccountRequest model);
    Task<AuthenticateUserResponse> AuthenticateUserAsync(AuthenticateUserRequest model);
    Task<AuthenticateUserResponse> RevokeTokenAsync(RevokeRefreshTokenRequest model);
}