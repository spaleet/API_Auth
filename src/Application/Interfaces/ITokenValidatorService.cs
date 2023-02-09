using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Application.Interfaces;

public interface ITokenValidatorService
{
    Task ValidateAsync(TokenValidatedContext context);
}