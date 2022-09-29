using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;
public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task RegisterAsync(RegisterAccountRequest model)
    {
        var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);

        if (userWithSameEmail != null)
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
}
