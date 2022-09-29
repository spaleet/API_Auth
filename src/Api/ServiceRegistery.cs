using Domain.Entities;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistery
{
    public static void ConfigureDb(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DatabaseContext>(
            opts => opts.UseSqlServer(connectionString,
            b => b.MigrationsAssembly("Infrastructure")));
    }

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, UserRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 0;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();
    }

}
