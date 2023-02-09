using Domain.Entities;
using Domain.Enums;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seeds;

public class DatabaseContextInitializer
{
    private readonly DatabaseContext _context;
    private readonly ILogger<DatabaseContextInitializer> _logger;
    private readonly RoleManager<UserRole> _roleManager;

    public DatabaseContextInitializer(ILogger<DatabaseContextInitializer> logger, DatabaseContext context,
        RoleManager<UserRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while initializing the database : {ex}", ex.Message);
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while seeding the database : {ex}", ex.Message);
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        if (!await _roleManager.RoleExistsAsync(Roles.Admin.ToString()))
            await _roleManager.CreateAsync(new UserRole
            {
                Name = Roles.Admin.ToString(),
                NormalizedName = Roles.Admin.ToString().ToUpper()
            });

        if (!await _roleManager.RoleExistsAsync(Roles.BasicUser.ToString()))
            await _roleManager.CreateAsync(new UserRole
            {
                Name = Roles.BasicUser.ToString(),
                NormalizedName = Roles.BasicUser.ToString().ToUpper()
            });
    }
}