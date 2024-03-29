﻿using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenFactoryService _tokenFactoryService;
    private readonly ITokenStoreService _tokenStoreService;
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager, SignInManager<User> signInManager,
        ITokenFactoryService tokenFactoryService, ITokenStoreService tokenStoreService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenFactoryService = tokenFactoryService;
        _tokenStoreService = tokenStoreService;
    }

    public async Task RegisterAsync(RegisterAccountRequest model)
    {
        var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);

        if (userWithSameEmail is not null)
            throw new ApiException("Email is not valid.");

        var user = new User
        {
            Email = model.Email,
            UserName = model.Username
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            throw new ApiException(result.Errors.First().Description);

        await _userManager.AddToRoleAsync(user, Roles.BasicUser.ToString());

        // Todo : Authorization
        await _userManager.AddToRoleAsync(user, Roles.Admin.ToString());
    }

    public async Task<AuthenticateUserResponse> AuthenticateUserAsync(AuthenticateUserRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null)
            throw new NotFoundException("No user was found.");

        var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);

        if (!result.Succeeded)
            throw new ApiException("Authentication failed.");

        var token = await _tokenFactoryService.CreateJwtTokenAsync(user);
        await _tokenStoreService.AddUserToken(user, token.RefreshTokenSerial, token.AccessToken);

        return new AuthenticateUserResponse(token);
    }

    public async Task<AuthenticateUserResponse> RevokeTokenAsync(RevokeRefreshTokenRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.RefreshToken))
            throw new ApiException("Token isn't valid!");

        var token = await _tokenStoreService.FindToken(model.RefreshToken);

        if (token == null)
            throw new ApiException("Token isn't valid!");

        var user = await _userManager.FindByIdAsync(token.UserId.ToString());

        if (user is null)
            throw new NotFoundException("No user was found.");

        var jwtResult = await _tokenFactoryService.CreateJwtTokenAsync(user);

        string? refreshTokenSerial = _tokenFactoryService.GetRefreshTokenSerial(model.RefreshToken);

        await _tokenStoreService.AddUserToken(user, jwtResult.RefreshTokenSerial, jwtResult.AccessToken,
            refreshTokenSerial);

        return new AuthenticateUserResponse(jwtResult);
    }
}